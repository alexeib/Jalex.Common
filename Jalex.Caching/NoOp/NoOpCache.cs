using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.NoOp
{
    internal class NoOpCache<TKey, TItem> : ICache<TKey, TItem>
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Task _noOpDeleter = Task.Factory.StartNew(() => { });

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

        public IEnumerable<KeyValuePair<TKey, TItem>> GetAll()
        {
            yield break;
        }

        public IEnumerable<KeyValuePair<TKey, TItem>> GetMany(IEnumerable<TKey> keys)
        {
            yield break;
        }

        public TItem Get(TKey key)
        {
            return default(TItem);
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
