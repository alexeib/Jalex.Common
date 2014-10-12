using System;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.Memory
{
    internal class MemoryCacheConfiguration : ICacheStrategyConfiguration
    {
        public string CacheName { get; private set; }
        public string NamedScope { get; private set; }
        public bool IsUsingTypedScope { get; private set; }

        internal MemoryCacheConfiguration()
        {
            CacheName = "@default";
        }


        ICacheStrategyConfiguration ICacheStrategyConfiguration.UseNamedCache(string name)
        {
            CacheName = name;
            return this;
        }

        /// <summary>
        /// It's like a view into a table.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>
        /// Don't use scoped and unscoped cache instances over the same named cache, define seprate named caches instead for scoped and unscoped items.
        /// </remarks>
        public ICacheStrategyConfiguration UseNamedScope(string name)
        {
            NamedScope = name;
            return this;
        }

        /// <summary>
        /// Generate scope name based on the cache type arguments.
        /// </summary>
        public ICacheStrategyConfiguration UseTypedScope()
        {
            IsUsingTypedScope = true;
            return this;
        }

        /// <summary>
        /// Overwrite default cache expiry policy.
        /// </summary>
        /// <param name="timeToKeep"></param>
        public ICacheStrategyConfiguration UseTimedExpiry(TimeSpan timeToKeep)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retry cache operations on exception.
        /// </summary>
        /// <param name="numberOfRetries"></param>
        public ICacheStrategyConfiguration UseRetries(int numberOfRetries)
        {
            // do nothing
            return this;
        }
    }
}
