using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jalex.Infrastructure.Caching
{
    /// <summary>
    ///     ICache abstract access to various cache implementations.
    /// </summary>
    /// <typeparam name="TKey">The entity type key.</typeparam>
    /// <typeparam name="TItem">
    ///     The entity type to cache. DateTimeKinds associated with any date value for this type will be fixed to Utc.
    /// </typeparam>
    public interface ICache<TKey, TItem> : IDisposable
    {

        /// <summary>
        ///     Gets a single entity by its unique identifier.
        /// </summary>
        /// <returns>Null if no matching entity exists.</returns>
        TItem Get(TKey key);

        /// <summary>
        ///     Adds/updates an item from cache.
        /// </summary>
        /// <param name="item">The entity to cache.</param>
        /// <param name="key">The entity's unique id.</param>
        void Set(TKey key, TItem item);

        /// <summary>
        ///     Retrieve key-value pairs for specified keys.
        /// </summary>
        /// <param name="keys">ids.</param>
        /// <returns>All items that match the keys.</returns>
        IEnumerable<KeyValuePair<TKey, TItem>> GetMany(IEnumerable<TKey> keys);

        /// <summary>
        ///     Retrieve  key-value pairs for all items.
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<TKey, TItem>> GetAll();

        /// <summary>
        ///     Removes the cached item from the collection.
        /// </summary>
        void DeleteById(TKey key);

        /// <summary>
        ///     Deletes all cached items in the collection.
        /// </summary>
        Task DeleteAll();

        /// <summary>
        ///     Get all keys in the cache.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TKey> GetKeys();

        /// <summary>
        ///     Get number of items in the cache.
        /// </summary>
        /// <returns></returns>
        long GetSize();
    }
}