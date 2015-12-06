using System;

namespace Jalex.Infrastructure.Caching
{
    /// <summary>
    /// Abstracts cache options configurator.
    /// </summary>
    public interface ICacheStrategyConfiguration
    {
        /// <summary>
        /// use the given name for the cache
        /// </summary>
        /// <param name="name"></param>
        ICacheStrategyConfiguration UseNamedCache(string name);

        /// <summary>
        /// Scope for the cache. Different scopes, but same names result in different caches
        /// </summary>
        ICacheStrategyConfiguration UseNamedScope(string name);

        /// <summary>
        /// Generate scope name based on the cache type arguments.
        /// </summary>
        ICacheStrategyConfiguration UseTypedScope();
    }
}
