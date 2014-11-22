using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.Memory
{
    /// <summary>
    /// A simple in memory cache
    /// </summary>
    /// <typeparam name="TCacheItem">The type of the object to cache.</typeparam>
    /// <typeparam name="TKKey">The type of key to use.</typeparam>
    public sealed class MemoryCache<TKKey, TCacheItem> : ICache<TKKey, TCacheItem>
    {
        private bool _disposed;

        private ConcurrentDictionary<TKKey, TCacheItem> _items = new ConcurrentDictionary<TKKey, TCacheItem>();

        /// <summary>
        ///     Clears the object cache.
        /// </summary>
        public Task DeleteAll()
        {
            return Task.Factory.StartNew(() => _items.Clear());
        }

        public IEnumerable<TKKey> GetKeys()
        {
            return _items.Keys;
        }

        public long GetSize()
        {
            return _items.Count;
        }

        /// <summary>
        ///     Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _items = null;
            }
        }

        /// <summary>
        ///     Removes the cached key from the collection.
        /// </summary>
        public void DeleteById(TKKey key)
        {
            TCacheItem removed;
            _items.TryRemove(key, out removed);
        }

        public IEnumerable<KeyValuePair<TKKey, TCacheItem>> GetMany(IEnumerable<TKKey> keys)
        {
            foreach (var key in keys)
            {
                TCacheItem item;
                if (_items.TryGetValue(key, out item))
                {
                    yield return new KeyValuePair<TKKey, TCacheItem>(key, item);
                }
            }
        }

        /// <summary>
        ///     Gets a single entity by its unique identifier.
        /// </summary>
        /// <returns>false if key is not in cache, true if it is</returns>
        public bool TryGet(TKKey key, out TCacheItem item)
        {
            return _items.TryGetValue(key, out item);
        }

        public void Set(TKKey key, TCacheItem item)
        {
            _items[key] = item;
        }

        public IEnumerable<KeyValuePair<TKKey, TCacheItem>> GetAll()
        {
            return _items.ToArray();
        }
    }
}
