using System;
using System.Collections.Generic;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.NoOp
{
    public class NoOpCacheFactory : ICacheFactory
    {
        public ICache<TKey, TItem> Create<TKey, TItem>(Action<ICacheStrategyConfiguration> configure) where TItem : class
        {
            return new NoOpCache<TKey, TItem>();
        }

        public IEnumerable<string> GetCacheNames()
        {
            yield break;
        }

        public CacheCapabilities GetCapabilities()
        {
            return CacheCapabilities.None;
        }

        public void Dispose()
        {
        }
    }
}
