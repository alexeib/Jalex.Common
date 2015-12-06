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
        /// Gets an object from cache or puts it there if it doesnt exist
        /// </summary>
        public static TItem GetOrAdd<TKey, TItem>(this ICache<TKey, TItem> cache, TKey key, Func<TKey, TItem> ifMissing)
        {
            Guard.AgainstNull(cache, "cache");
            Guard.AgainstNull(ifMissing, "ifMissing");

            TItem item;
            var success = cache.TryGet(key, out item);
            if (!success)
            {
                item = ifMissing(key);
                cache.Set(key, item);
            }
            return item;
        }

        public static TItem GetOrDefault<TKey, TItem>(this ICache<TKey, TItem> cache, TKey key)
        {
            Guard.AgainstNull(cache, "cache");

            TItem item;
            cache.TryGet(key, out item);
            return item;
        }

        public static Task SetMany<TKey, TItem>(this ICache<TKey, TItem> cache, IEnumerable<TItem> items, Func<TItem, TKey> getKey)
        {
            Guard.AgainstNull(cache, "cache");
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.AgainstNull(items, "items");
            Guard.AgainstNull(getKey, "getKey");

            return Task.Factory.StartNew(() =>
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var i in items)
                    cache.Set(getKey(i), i);
            });
        }

        public static Task DeleteMany<TKey, TItem>(this ICache<TKey, TItem> cache, IEnumerable<TKey> keys)
        {
            Guard.AgainstNull(cache, "cache");
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.AgainstNull(keys, "items");

            return Task.Factory.StartNew(() =>
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var key in keys)
                    cache.DeleteById(key);
            });
        }
    }
}
