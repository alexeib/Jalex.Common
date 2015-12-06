using System;
using System.Collections.Generic;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.NoOp
{
    public class NoOpCacheFactory : ICacheFactory
    {
        public ICache<TKey, TItem> Create<TKey, TItem>(Action<ICacheStrategyConfiguration> configure)
        {
            return new NoOpCache<TKey, TItem>();
        }

        public void Dispose()
        {
        }
    }
}
