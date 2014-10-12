using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Magnum;

namespace Jalex.Infrastructure.Caching
{
    /// <summary>
    /// IEnumerableEx that should be applicable to all ICache implementations.
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Gets the value out of the cache or stores one produced by the supplied function and returns it.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="ifMissing">factory callback to produce new instance if one with the specified key is missing</param>
        /// <returns></returns>
        public static TItem Get<TKey,TItem>(this ICache<TKey, TItem> cache, TKey key, Func<TKey, TItem> ifMissing) where TItem : class
        {
            Guard.AgainstNull(cache, "cache");
            Guard.AgainstNull(ifMissing, "ifMissing");

            var item = cache.Get(key);
            if (item == default(TItem))
            {
                item = ifMissing(key);
                if (item != default(TItem))
                {
                    cache.Set(key, item);
                }
            }
            return item;
        }

        public static Task SetMany<TKey, TItem>(this ICache<TKey, TItem> cache, IEnumerable<TItem> items, Func<TItem, TKey> getKey)
            where TItem : class
        {
            Guard.AgainstNull(cache, "cache");
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.AgainstNull(items, "items");
            Guard.AgainstNull(getKey, "getKey");

            return Task.Factory.StartNew(() =>
            {
                foreach (var i in items)
                    cache.Set(getKey(i), i);
            });
        }
    }
}
