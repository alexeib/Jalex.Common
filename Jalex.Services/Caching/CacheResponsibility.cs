using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Logging;
using Magnum;

namespace Jalex.Services.Caching
{
    public class CacheResponsibility<T> : IQueryableRepository<T>
        where T : class
    {
        private readonly ICache<Guid, T> _keyCache;        
        private readonly IQueryableRepository<T> _repository;
        private readonly IReflectedTypeDescriptor<T> _typeDescriptor;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly ILogger _staticLogger = LogManager.GetCurrentClassLogger();
        private ILogger _instanceLogger;

        public ILogger Logger
        {
            get { return _instanceLogger ?? _staticLogger; }
            set { _instanceLogger = value; }
        }

        public CacheResponsibility(
            IQueryableRepository<T> repository,
            IReflectedTypeDescriptorProvider typeDescriptorProvider,
            ICacheFactory cacheFactory,            
            Action<ICacheStrategyConfiguration> cacheConfiguration)
        {
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(typeDescriptorProvider, "typeDescriptorProvider");
            Guard.AgainstNull(cacheFactory, "cacheFactory");
            Guard.AgainstNull(cacheConfiguration, "cacheConfiguration");

            _repository = repository;
            _keyCache = cacheFactory.Create<Guid, T>(cacheConfiguration);            
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
        }


        #region Implementation of IReader<out TEntity>

        /// <summary>
        /// Retrieves an object by id. 
        /// </summary>
        public async Task<T> GetByIdAsync(Guid id)
        {
            T entity;
            var success = _keyCache.TryGet(id, out entity);
            if (!success)
            {
                entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);
                if (entity != null)
                {
                    cacheItem(entity);
                }
            }

            return entity;
        }

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync().ConfigureAwait(false);
            foreach (var item in items)
            {
                cacheItem(item);
            }
            return items;
        }

        #endregion

        #region Implementation of IDeleter<TEntity>

        public async Task<OperationResult> DeleteAsync(Guid id)
        {
            var result = await _repository.DeleteAsync(id).ConfigureAwait(false);

            if (result.Success)
            {
                _keyCache.DeleteById(id);
            }

            return result;
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public async Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression)
        {
            var items = await _repository.QueryAsync(expression).ConfigureAwait(false);
            var result = await _repository.DeleteWhereAsync(expression).ConfigureAwait(false);

            if (result.Success)
            {
                foreach (var item in items)
                {
                    _keyCache.DeleteById(_typeDescriptor.GetId(item));
                }
            }

            return result;
        }

        #endregion

        #region Implementation of IWriter<in TEntity>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public async Task<OperationResult<Guid>> SaveAsync(T obj, WriteMode writeMode)
        {
            var result = await _repository.SaveAsync(obj, writeMode).ConfigureAwait(false);
            if (result.Success)
            {
                cacheItem(obj);
            }
            return result;
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public async Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode)
        {
            var objArr = objects.ToArrayEfficient();

            var results = (await _repository.SaveManyAsync(objArr, writeMode).ConfigureAwait(false)).ToArrayEfficient();

            if (objArr.Length != results.Length)
            {
                throw new InvalidDataException(string.Format("repository returned {0} results when saving {1} objects", results.Length, objArr.Length));
            }

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].Success)
                {
                    cacheItem(objArr[i]);
                }
            }

            return results;
        }

        #endregion

        #region Implementation of IQueryableReader<TEntity>

        public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query)
        {
            var items = (await _repository.QueryAsync(query).ConfigureAwait(false)).ToCollection();
            foreach (var item in items)
            {
                cacheItem(item);
            }
            return items;
        }

        public Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> query)
        {
            return _repository.ProjectAsync(projection, query);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query)
        {
            var item = await _repository.FirstOrDefaultAsync(query).ConfigureAwait(false);
            cacheItem(item);
            return item;
        }

        #endregion

        private void cacheItem(T item)
        {
            if (item == null)
            {
                return;
            }

            var id = _typeDescriptor.GetId(item);

            if (id == Guid.Empty)
            {
                return;
            }

            _keyCache.Set(id, item);
        }
    }
}
