using System.Collections.Generic;

namespace Jalex.Services.Caching
{
    public interface IIndexCacheFactory
    {
        IEnumerable<IIndexCache<T>> CreateIndexCachesForType<T>();
    }
}