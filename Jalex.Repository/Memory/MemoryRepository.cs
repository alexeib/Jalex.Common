using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.IdProviders;

namespace Jalex.Repository.Memory
{
    public class MemoryRepository<T> : BaseRepository<T>, IQueryableRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _objectDictionary;

        public MemoryRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
            : base(idProvider, typeDescriptorProvider)
        {
            _objectDictionary = new ConcurrentDictionary<Guid, T>();
        }

        #region Implementation of IReader<out T>

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_objectDictionary.GetValueOrDefault(id));
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            var objects = _objectDictionary.Values.ToArray();
            return Task.FromResult<IEnumerable<T>>(objects);
        }

        #endregion

        #region Implementation of IDeleter<T>

        public Task<OperationResult> DeleteAsync(Guid id)
        {
            T obj;
            bool success = _objectDictionary.TryRemove(id, out obj);
            return Task.FromResult(new OperationResult(success));
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression)
        {
            var items = QueryAsync(expression).Result.ToCollection();

            // ReSharper disable once TooWideLocalVariableScope
            T obj;
            foreach (var item in items)
            {
                var id = _typeDescriptor.GetId(item);
                _objectDictionary.TryRemove(id, out obj);
            }
            return Task.FromResult(new OperationResult(true));
        }

        #endregion

        #region Implementation of IQueryableReader<T>

        public Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query)
        {
            var queryableValues = _objectDictionary.Values.AsQueryable();
            var results = queryableValues.Where(query);
            return Task.FromResult<IEnumerable<T>>(results);
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query)
        {
            return Task.FromResult(QueryAsync(query).Result.FirstOrDefault());
        }

        #endregion

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public Task<OperationResult<Guid>> SaveAsync(T obj, WriteMode writeMode)
        {
            return Task.FromResult(SaveManyAsync(new[] { obj }, writeMode).Result.Single());
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode)
        {
            Guard.AgainstNull(objects, "objects");

            var objectArr = objects as T[] ?? objects.ToArray();
            var results = createResults(
                writeMode, objectArr,
                id => _objectDictionary.ContainsKey(id),
                obj =>
                {
                    _objectDictionary[_typeDescriptor.GetId(obj)] = obj;
                    return true;
                });

            return Task.FromResult(results);
        }

        #endregion

        private IEnumerable<OperationResult<Guid>> createResults(
            WriteMode writeMode,
            IReadOnlyCollection<T> objects,
            Func<Guid, bool> doesObjectWithIdExist,
            Func<T, bool> actualAdd)
        {
            try
            {
                ensureObjectIds(writeMode, objects);
            }
            catch (Exception e)
            {
                throw new AggregateException(e);
            }            

            return (from obj in objects
                    let id = _typeDescriptor.GetId(obj)
                    select createResult(writeMode, doesObjectWithIdExist, actualAdd, id, obj))
                .ToList();
        }

        private OperationResult<Guid> createResult(WriteMode writeMode, Func<Guid, bool> doesObjectWithIdExist, Func<T, bool> actualAdd, Guid id, T newObj)
        {
            OperationResult<Guid> failedResult;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!checkIfCanWrite(writeMode, id, doesObjectWithIdExist, out failedResult))
            {
                return failedResult;
            }
            try
            {
                if (actualAdd(newObj))
                {
                    return new OperationResult<Guid>(true, id);
                }
                return new OperationResult<Guid>(
                                false,
                                id,
                                Severity.Warning,
                                string.Format("Failed to save {0} {1}", _typeDescriptor.TypeName, id));
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "Failed to save (mode={0}) {1} (id={2})", writeMode, _typeDescriptor.TypeName, id);
                return new OperationResult<Guid>(
                                false,
                                id,
                                Severity.Error,
                                string.Format("Failed to add save {0} (id={1})", _typeDescriptor.TypeName, id));

            }
        }

        private bool checkIfCanWrite(WriteMode writeMode, Guid id, Func<Guid, bool> doesObjectWithIdExist, out OperationResult<Guid> failedResult)
        {
            switch (writeMode)
            {
                case WriteMode.Insert:

                    if (id != Guid.Empty && doesObjectWithIdExist(id))
                    {
                        string message = string.Format("{0} with id {1} already exists", _typeDescriptor.TypeName, id);
                        failedResult = new OperationResult<Guid>(false, id, Severity.Error, message);
                        Logger.Warn(message);
                        return false;
                    }
                    break;
                case WriteMode.Update:
                    if (id == Guid.Empty || !doesObjectWithIdExist(id))
                    {
                        string message = string.Format("{0} with id {1} does not exist", _typeDescriptor.TypeName, id);
                        failedResult = new OperationResult<Guid>(false, id, Severity.Error, message);
                        Logger.Warn(message);
                        return false;
                    }
                    break;
                case WriteMode.Upsert:
                    // nothing to check
                    break;
                default:
                    throw new ArgumentOutOfRangeException("writeMode");
            }

            failedResult = null;
            return true;
        }
    }
}
