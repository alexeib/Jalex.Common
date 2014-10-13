using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;

namespace Jalex.Services.Caching
{
    public class IndexCacheFactory : IIndexCacheFactory
    {
        private readonly ICacheFactory _cacheFactory;
        private readonly Action<ICacheStrategyConfiguration> _cacheConfiguration;
        private readonly IReflectedTypeDescriptorProvider _reflectedTypeDescriptorProvider;

        public IndexCacheFactory(
            ICacheFactory cacheFactory,
            Action<ICacheStrategyConfiguration> cacheConfiguration,
            IReflectedTypeDescriptorProvider reflectedTypeDescriptorProvider)
        {
            Guard.AgainstNull(cacheFactory, "cacheFactory");
            Guard.AgainstNull(cacheConfiguration, "cacheConfiguration");
            Guard.AgainstNull(reflectedTypeDescriptorProvider, "reflectedTypeDescriptorProvider");

            _cacheFactory = cacheFactory;
            _cacheConfiguration = cacheConfiguration;
            _reflectedTypeDescriptorProvider = reflectedTypeDescriptorProvider;
        }

        #region Implementation of IIndexCacheFactory

        public IEnumerable<IIndexCache<T>> CreateIndexCachesForType<T>()
        {
            var reflectedTypeDescrptor = _reflectedTypeDescriptorProvider.GetReflectedTypeDescriptor<T>();
            
            // for now just index clustered indices. In the future we may support unique indices as well
            if (reflectedTypeDescrptor.HasClusteredIndices)
            {
                yield return createClusteredIndexCache(reflectedTypeDescrptor);
            }
        }        

        #endregion

        private IIndexCache<T> createClusteredIndexCache<T>(IReflectedTypeDescriptor<T> reflectedTypeDescrptor)
        {
            var clusteredIndexPropNames =
                reflectedTypeDescrptor
                    .Properties
                    .Where(prop => prop.GetCustomAttributes(true).Any(a => a is IndexedAttribute && ((IndexedAttribute) a).IsClustered))
                    .Select(prop => prop.Name);

            var indexCache = new IndexCache<T>(clusteredIndexPropNames, _cacheFactory, _cacheConfiguration, _reflectedTypeDescriptorProvider);
            return indexCache;
        }
    }
}