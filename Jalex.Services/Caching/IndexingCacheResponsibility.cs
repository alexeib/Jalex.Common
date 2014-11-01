using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;

namespace Jalex.Services.Caching
{
    public class IndexingCacheResponsibility<T> : CacheResponsibility<T>
        where T : class
    {
        public IndexingCacheResponsibility(IQueryableRepository<T> repository, IReflectedTypeDescriptorProvider typeDescriptorProvider, ICacheFactory cacheFactory, IIndexCacheFactory indexCacheFactory, Action<ICacheStrategyConfiguration> cacheConfiguration) : base(repository, typeDescriptorProvider, cacheFactory, indexCacheFactory, cacheConfiguration)
        {
        }
    }
}
