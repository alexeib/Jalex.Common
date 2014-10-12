using System;

namespace Jalex.Infrastructure.Caching
{
    /// <summary>
    /// Abstracts cache options configurator.
    /// </summary>
    public interface ICacheStrategyConfiguration
    {
        /// <summary>
        /// It's like an external table.
        /// </summary>
        /// <param name="name"></param>
        ICacheStrategyConfiguration UseNamedCache(string name);

        /// <summary>
        /// It's like a view into a table.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>
        /// Don't use scoped and unscoped cache instances over the same named cache, define seprate named caches instead for scoped and unscoped items.
        /// </remarks>
        ICacheStrategyConfiguration UseNamedScope(string name);

        /// <summary>
        /// Generate scope name based on the cache type arguments.
        /// </summary>
        ICacheStrategyConfiguration UseTypedScope();

        /// <summary>
        /// Overwrite default cache expiry policy.
        /// </summary>
        /// <param name="timeToKeep"></param>
        ICacheStrategyConfiguration UseTimedExpiry(TimeSpan timeToKeep);

        /// <summary>
        /// Retry cache operations on exception.
        /// </summary>
        /// <param name="numberOfRetries"></param>
        ICacheStrategyConfiguration UseRetries(int numberOfRetries);
    }
}
