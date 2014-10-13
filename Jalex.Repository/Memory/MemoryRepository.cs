using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.IdProviders;

namespace Jalex.Repository.Memory
{
    public class MemoryRepository<T> : BaseRepository<T>, IQueryableRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> _objectDictionary;

        public MemoryRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
            : base(idProvider, typeDescriptorProvider)
        {
            _objectDictionary = new ConcurrentDictionary<string, T>();
        }

        #region Implementation of IReader<out T>

        public bool TryGetById(string id, out T obj)
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

        public OperationResult Delete(string id)
        {
            T obj;
            bool success = _objectDictionary.TryRemove(id, out obj);
            return new OperationResult(success);
        }

        #endregion

        #region Implementation of IQueryable<T>

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
        public OperationResult<string> Save(T obj, WriteMode writeMode)
        {
            return SaveMany(new[] { obj }, writeMode).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<string>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            ParameterChecker.CheckForNull(objects, "objects");

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
