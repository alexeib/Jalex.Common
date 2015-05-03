using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Logging;

namespace Jalex.Services.Caching
{
    public class IndexCacheResponsibility<T> :  IQueryableRepository<T>
        where T : class
    {
        private readonly IEnumerable<IIndexCache<T>> _indexCaches;
        private readonly IQueryableRepository<T> _repository;
        private readonly IReflectedTypeDescriptor<T> _typeDescriptor;


        public IndexCacheResponsibility(
            IQueryableRepository<T> repository,
            IIndexCacheFactory indexCacheFactory, 
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
        {
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(indexCacheFactory, "indexCacheFactory");
            Guard.AgainstNull(typeDescriptorProvider, "typeDescriptorProvider");

            _repository = repository;
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
            _indexCaches = indexCacheFactory.CreateIndexCachesForType<T>();
        }

        #region Implementation of IReader<T>

        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <param name="obj"> retrieved object</param>
        /// <returns>True if retrieval succeeded, false otherwise</returns>
        public bool TryGetById(Guid id, out T obj)
        {
            var success = _repository.TryGetById(id, out obj);
            if (success)
            {
                cacheItem(obj);
            }
            return success;
        }

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        public IEnumerable<T> GetAll()
        {
            foreach (var item in _repository.GetAll())
            {
                cacheItem(item);
                yield return item;
            }
        }

        #endregion

        #region Implementation of IDeleter<T>

        /// <summary>
        /// Deletes an existing object
        /// </summary>
        /// <param name="id">The id of the object to delete</param>
        /// <returns>the result of the delete operation</returns>
        public OperationResult Delete(Guid id)
        {
            deIndexItemWithId(id);
            var result = _repository.Delete(id);
            return result;
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public OperationResult DeleteWhere(Expression<Func<T, bool>> expression)
        {
            T retrieved;
            var success = tryGetCachedObjectFromQuery(expression, out retrieved);
            if (success)
            {
                foreach (var indexCache in _indexCaches)
                {
                    indexCache.DeIndex(retrieved);
                }
            }

            var result = _repository.DeleteWhere(expression);
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
        public OperationResult<Guid> Save(T obj, WriteMode writeMode)
        {
            if (writeMode != WriteMode.Insert)
            {
                Guid id = _typeDescriptor.GetId(obj);
                deIndexItemWithId(id);
            }

            var result = _repository.Save(obj, writeMode);
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
        public IEnumerable<OperationResult<Guid>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            var objArr = objects.ToArrayEfficient();

            if (writeMode != WriteMode.Insert)
            {
                foreach (var obj in objArr)
                {
                    Guid id = _typeDescriptor.GetId(obj);
                    deIndexItemWithId(id);
                }
            }

            var results = _repository.SaveMany(objArr, writeMode).ToArrayEfficient();

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

        #region Implementation of IInjectableLogger

        // ReSharper disable once StaticFieldInGenericType
        private static readonly ILogger _staticLogger = LogManager.GetCurrentClassLogger();
        private ILogger _instanceLogger;

        public ILogger Logger
        {
            get { return _instanceLogger ?? _staticLogger; }
            set { _instanceLogger = value; }
        }

        #endregion

        #region Implementation of IQueryableReader<T>

        /// <summary>
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            T retrieved;
            var success = tryGetCachedObjectFromQuery(query, out retrieved);
            if (success)
            {
                yield return retrieved;
                yield break;
            }

            var items = _repository.Query(query);
            foreach (var item in items)
            {
                cacheItem(item);
                yield return item;
            }
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public T FirstOrDefault(Expression<Func<T, bool>> query)
        {
            T retrieved;
            var success = tryGetCachedObjectFromQuery(query, out retrieved);
            if (success)
            {
                return retrieved;
            }

            var item = _repository.FirstOrDefault(query);
            cacheItemAndIndexIfNull(item, query);
            return item;
        }

        #endregion

        private bool cacheItem(T item)
        {
            if (item == null)
            {
                return false;
            }

            foreach (var indexCache in _indexCaches)
            {
                indexCache.Index(item);
            }

            return true;
        }

        private void deIndexItemWithId(Guid id)
        {
            if (id == Guid.Empty)
            {
                return;
            }

            T obj;
            if (TryGetById(id, out obj))
            {
                foreach (var indexCache in _indexCaches)
                {
                    indexCache.DeIndex(obj);
                }
            }
        }

        private void cacheItemAndIndexIfNull(T item, Expression<Func<T, bool>> query)
        {
            var cached = cacheItem(item);

            // item or key is null - index this fact
            if (!cached)
            {
                foreach (var indexCache in _indexCaches)
                {
                    indexCache.IndexByQuery(query, Guid.Empty);
                }
            }
        }

        private bool tryGetCachedObjectFromQuery(Expression<Func<T, bool>> query, out T retrieved)
        {
            if (!_indexCaches.Any())
            {
                retrieved = default(T);
                return false;
            }

            foreach (var indexCache in _indexCaches)
            {
                var objId = indexCache.FindIdByQuery(query);
                if (objId != Guid.Empty && _repository.TryGetById(objId, out retrieved))
                {
                    return true;
                }
            }

            retrieved = default(T);
            return false;
        }
    }
}
