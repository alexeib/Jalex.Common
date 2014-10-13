using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly ICache<string, T> _keyCache;
        private readonly IEnumerable<IIndexCache<T>> _indexCaches;
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
            IIndexCacheFactory indexCacheFactory,
            Action<ICacheStrategyConfiguration> cacheConfiguration)
        {
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(typeDescriptorProvider, "typeDescriptorProvider");
            Guard.AgainstNull(cacheFactory, "cacheFactory");
            Guard.AgainstNull(indexCacheFactory, "indexCacheFactory");
            Guard.AgainstNull(cacheConfiguration, "cacheConfiguration");

            _repository = repository;
            _keyCache = cacheFactory.Create<string, T>(cacheConfiguration);
            _indexCaches = indexCacheFactory.CreateIndexCachesForType<T>();
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
        }


        #region Implementation of IReader<out TEntity>

        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <param name="entity">the retrieved object</param>
        /// <returns>True if retrieval succeeded, false otherwise</returns>
        public bool TryGetById(string id, out T entity)
        {
            var success = _keyCache.TryGet(id, out entity);
            if (!success)
            {
                success = tryGetFromRepositoryByIdAndCache(id, out entity);
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

        #region Implementation of IDeleter<TEntity>

        public OperationResult Delete(string id)
        {
            var result = _repository.Delete(id);

            if (result.Success)
            {
                _keyCache.DeleteById(id);
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
        public OperationResult<string> Save(T obj, WriteMode writeMode)
        {
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
        public IEnumerable<OperationResult<string>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            var objArr = objects.ToArrayEfficient();
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

        #region Implementation of IQueryable<TEntity>

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

        public T FirstOrDefault(Expression<Func<T, bool>> query)
        {
            T retrieved;
            var success = tryGetCachedObjectFromQuery(query, out retrieved);
            if (success)
            {
                return retrieved;
            }

            var item = _repository.FirstOrDefault(query);
            cacheItem(item);
            return item;
        }

        #endregion

        private bool tryGetFromRepositoryByIdAndCache(string id, out T entity)
        {
            var success = _repository.TryGetById(id, out entity);
            if (success)
            {
                cacheItem(entity);
            }
            return success;
        }

        private void cacheItem(T item)
        {
            if (item == null)
            {
                return;
            }

            var id = _typeDescriptor.GetId(item);

            if (id == null)
            {
                return;
            }

            _keyCache.Set(id, item);
            foreach (var indexCache in _indexCaches)
            {
                indexCache.Index(item);
            }
        }

        private bool tryGetCachedObjectFromQuery(Expression<Func<T, bool>> query, out T retrieved)
        {
            if (!_indexCaches.Any())
            {
                retrieved = default(T);
                return false;
            }

            var compiledQuery = query.Compile();

            foreach (var indexCache in _indexCaches)
            {
                var objId = indexCache.FindIdByQuery(query);
                if (objId != null)
                {
                    var success = TryGetById(objId, out retrieved);
                    if (success)
                    {
                        if (compiledQuery(retrieved))
                        {
                            //success
                            return true;
                        }

                        // stale index
                        indexCache.DeIndexByQuery(query);
                        indexCache.Index(retrieved);
                    }
                }
            }

            retrieved = default(T);
            return false;
        }
    }
}
