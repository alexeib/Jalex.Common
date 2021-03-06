﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.NoOp
{
    internal class NoOpCache<TKey, TItem> : ICache<TKey, TItem>
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Task _noOpDeleter = Task.Factory.StartNew(() => { });

        /// <summary>
        ///     Gets a single entity by its unique identifier.
        /// </summary>
        /// <returns>false if key is not in cache, true if it is</returns>
        public bool TryGet(TKey key, out TItem item)
        {
            item = default(TItem);
            return false;
        }

        public void Set(TKey key, TItem item)
        {
        }

        public void DeleteById(TKey key)
        {
        }

        public Task DeleteAll()
        {
            return _noOpDeleter;
        }

        public IEnumerable<TKey> GetKeys()
        {
            yield break;
        }

        public long GetSize()
        {
            return 0;
        }

        public bool Contains(TKey key)
        {
            return false;
        }

        public IEnumerable<KeyValuePair<TKey, TItem>> GetAll()
        {
            yield break;
        }

        public IEnumerable<KeyValuePair<TKey, TItem>> GetMany(IEnumerable<TKey> keys)
        {
            yield break;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // noop
        }

        #endregion
    }
}
