using System;
using System.Collections.Generic;

namespace Jalex.Infrastructure.Caching
{
    public interface ICacheFactory : IDisposable
    {
        /// <summary>
        ///     Create new or return existing cache matching parameters.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        ICache<TKey, TItem> Create<TKey, TItem>(Action<ICacheStrategyConfiguration> configure) where TItem : class;

        /// <summary>
        ///     Obtain list of available named caches.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetCacheNames();

        /// <summary>
        ///     Obtain cache capabilities.
        /// </summary>
        /// <returns></returns>
        CacheCapabilities GetCapabilities();
    }
}