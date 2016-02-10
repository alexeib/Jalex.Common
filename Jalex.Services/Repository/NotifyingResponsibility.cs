using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Messaging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Repository.Messages;

namespace Jalex.Services.Repository
{
    public class NotifyingResponsibility<T> : IQueryableRepository<T> where T : class
    {
        class ChangingEntity
        {
            internal T Existing { get; }
            internal T New { get; }

            public ChangingEntity(T existing, T @new)
            {
                Existing = existing;
                New = @new;
            }
        }

        private readonly IQueryableRepository<T> _repository;
        private readonly IMessagePipe<EntityCreated<T>> _entityCreatedPipe;
        private readonly IMessagePipe<EntityUpdated<T>> _entityUpdatedPipe;
        private readonly IMessagePipe<EntityDeleted<T>> _entityDeletedPipe;
        private readonly IReflectedTypeDescriptor<T> _typeDescriptor;

        public NotifyingResponsibility(IQueryableRepository<T> repository,
                                       IReflectedTypeDescriptorProvider typeDescriptorProvider,
                                       IMessagePipe<EntityCreated<T>> entityCreatedPipe,
                                       IMessagePipe<EntityUpdated<T>> entityUpdatedPipe,
                                       IMessagePipe<EntityDeleted<T>> entityDeletedPipe)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (typeDescriptorProvider == null) throw new ArgumentNullException(nameof(typeDescriptorProvider));
            if (entityCreatedPipe == null) throw new ArgumentNullException(nameof(entityCreatedPipe));
            if (entityUpdatedPipe == null) throw new ArgumentNullException(nameof(entityUpdatedPipe));
            if (entityDeletedPipe == null) throw new ArgumentNullException(nameof(entityDeletedPipe));

            _repository = repository;
            _entityCreatedPipe = entityCreatedPipe;
            _entityUpdatedPipe = entityUpdatedPipe;
            _entityDeletedPipe = entityDeletedPipe;

            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
        }

        #region Implementation of IReader<T>

        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <returns>The item being retrieved or default(T) if it was not found</returns>
        public Task<T> GetByIdAsync(Guid id)
        {
            return _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        public Task<IEnumerable<T>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        #endregion

        #region Implementation of IDeleter<T>

        /// <summary>
        /// Deletes an updated object
        /// </summary>
        /// <param name="id">The id of the object to delete</param>
        /// <returns>the result of the delete operation</returns>
        public async Task<OperationResult> DeleteAsync(Guid id)
        {
            var idEquals = _typeDescriptor.GetExpressionForIdEquality(id);
            var items = (await _repository.QueryAsync(idEquals)
                                          .ConfigureAwait(false)).ToCollection();
            var result = await _repository.DeleteAsync(id)
                                          .ConfigureAwait(false);
            if (result.Success)
            {
                var messageTasks = items.Select(item => _entityDeletedPipe.SendAsync(new EntityDeleted<T>(item)));
                await Task.WhenAll(messageTasks)
                          .ConfigureAwait(false);
            }
            return result;
        }

        #endregion

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public async Task<OperationResult<Guid>> SaveAsync(T obj, WriteMode writeMode)
        {
            var saveManyResults = await SaveManyAsync(new[] {obj}, writeMode)
                                            .ConfigureAwait(false);
            return saveManyResults.Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public async Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));

            var objCollection = objects.ToCollection();

            ConcurrentBag<T> insertedEntities = new ConcurrentBag<T>();
            ConcurrentBag<ChangingEntity> updatedEntities = new ConcurrentBag<ChangingEntity>();

            switch (writeMode)
            {
                case WriteMode.Insert:
                    insertedEntities.AddRange(objCollection);
                    break;
                case WriteMode.Update:
                    var getExistingTasks = objCollection.Select(o => getExistingAsync(o, updatedEntities.Add));
                    await Task.WhenAll(getExistingTasks)
                              .ConfigureAwait(false);
                    break;
                case WriteMode.Upsert:
                    var tasks = objCollection.Select(o => getExistingAsync(o, updatedEntities.Add, insertedEntities.Add));
                    await Task.WhenAll(tasks)
                              .ConfigureAwait(false);
                    break;
            }
            
            var results = (await _repository.SaveManyAsync(objCollection, writeMode)).ToCollection();

            var successful = results.Where(r => r.Success)
                                    .Select(r => r.Value)
                                    .ToHashSet();

            var notificationTasks = insertedEntities.Where(i => successful.Contains(_typeDescriptor.GetId(i)))
                                                    .Select(i => _entityCreatedPipe.SendAsync(new EntityCreated<T>(i)))
                                                    .Concat(updatedEntities.Where(u => successful.Contains(_typeDescriptor.GetId(u.New)))
                                                                           .Select(u => _entityUpdatedPipe.SendAsync(new EntityUpdated<T>(u.Existing, u.New))));

            await Task.WhenAll(notificationTasks)
                      .ConfigureAwait(false);


            return results;
        }

        private async Task getExistingAsync(T obj, Action<ChangingEntity> gotExisting, Action<T> didNotGetExisting = null)
        {
            var id = _typeDescriptor.GetId(obj);
            T existing = null;

            if (id != Guid.Empty)
            {
                existing = await _repository.GetByIdAsync(id)  // TODO fix for cassandra's composite keys
                                            .ConfigureAwait(false);
            }

            if (existing == null)
            {
                didNotGetExisting?.Invoke(obj);
            }
            else
            {
                gotExisting(new ChangingEntity(existing, obj));
            }
        }

        #endregion

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public async Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression)
        {
            var itemsToBeDeleted = await _repository.QueryAsync(expression)
                                                    .ConfigureAwait(false);
            var result = await _repository.DeleteWhereAsync(expression)
                                          .ConfigureAwait(false);
            if (result.Success)
            {
                var notificationTasks = itemsToBeDeleted.Select(i => _entityDeletedPipe.SendAsync(new EntityDeleted<T>(i)));
                await Task.WhenAll(notificationTasks)
                          .ConfigureAwait(false);
            }

            return result;
        }


        #region Implementation of IQueryableReader<T>

        /// <summary>
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        public Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query)
        {
            return _repository.QueryAsync(query);
        }

        /// <summary>
        /// Projects a subset of an object that satisfy the given query
        /// </summary>
        public Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> query)
        {
            return _repository.ProjectAsync(projection, query);
        }

        /// <summary>
        /// Projects a subset of all objects in the repository
        /// </summary>
        public Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection)
        {
            return _repository.ProjectAsync(projection);
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query)
        {
            return _repository.FirstOrDefaultAsync(query);
        }

        #endregion
    }
}