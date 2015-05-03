using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public bool TryGetById(Guid id, out T obj)
        {
            return _objectDictionary.TryGetValue(id, out obj);
        }

        public IEnumerable<T> GetAll()
        {
            var objects = _objectDictionary.Values.ToArray();
            return objects;
        }

        #endregion

        #region Implementation of IDeleter<T>

        public OperationResult Delete(Guid id)
        {
            T obj;
            bool success = _objectDictionary.TryRemove(id, out obj);
            return new OperationResult(success);
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public OperationResult DeleteWhere(Expression<Func<T, bool>> expression)
        {
            var items = Query(expression)
                .ToCollection();
            foreach (var item in items)
            {
                var id = _typeDescriptor.GetId(item);
                Delete(id);
            }
            return new OperationResult(true);
        }

        #endregion

        #region Implementation of IQueryableReader<T>

        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            var queryableValues = _objectDictionary.Values.AsQueryable();
            var results = queryableValues.Where(query);
            return results;
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public T FirstOrDefault(Expression<Func<T, bool>> query)
        {
            return Query(query).FirstOrDefault();
        }

        #endregion

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public OperationResult<Guid> Save(T obj, WriteMode writeMode)
        {
            return SaveMany(new[] { obj }, writeMode).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<Guid>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
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

            return results;
        }

        #endregion
    }
}
