using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Magnum;

namespace Jalex.Services.Repository
{
    public class CacheResponsibility<TEntity> : IQueryableRepository<TEntity> 
    {
        private readonly ICache<string, TEntity> _keyCache;
        private readonly IQueryableRepository<TEntity> _repository;
        private readonly IReflectedTypeDescriptorProvider _typeDescriptorProvider;

        public CacheResponsibility(
            IQueryableRepository<TEntity> repository,
            IReflectedTypeDescriptorProvider typeDescriptorProvider,
            ICacheFactory cacheFactory,
            Action<ICacheStrategyConfiguration> cacheConfiguration)
        {
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(typeDescriptorProvider, "typeDescriptorProvider");
            Guard.AgainstNull(cacheFactory, "cacheFactory");
            Guard.AgainstNull(cacheConfiguration, "cacheConfiguration");

            _repository = repository;
            _typeDescriptorProvider = typeDescriptorProvider;
            _keyCache = createKeyCache(cacheFactory, cacheConfiguration);
        }

        private ICache<string, TEntity> createKeyCache(ICacheFactory cacheFactory, Action<ICacheStrategyConfiguration> cacheConfiguration)
        {
            throw new NotImplementedException();
        }

        private bool tryGetFromRepositoryByIdAndCache(string id, out TEntity entity)
        {
            throw new NotImplementedException();
        }

        private void cacheItem(TEntity item)
        {
            throw new NotImplementedException();
        }

        private string getEntityId(TEntity entity)
        {
            throw new NotImplementedException();
        }

        #region Implementation of IReader<out TEntity>

        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <param name="entity">the retrieved object</param>
        /// <returns>True if retrieval succeeded, false otherwise</returns>
        public bool TryGetById(string id, out TEntity entity)
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
        public IEnumerable<TEntity> GetAll()
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

        #region Implementation of IInjectableLogger

        public ILogger Logger { get; set; }

        #endregion

        #region Implementation of IQueryable<TEntity>

        public IEnumerable<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            throw new NotImplementedException();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IWriter<in TEntity>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public OperationResult<string> Save(TEntity obj, WriteMode writeMode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<string>> SaveMany(IEnumerable<TEntity> objects, WriteMode writeMode)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
