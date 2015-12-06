using System;

namespace Jalex.Infrastructure.Caching
{
    public interface ICacheFactory : IDisposable
    {
        /// <summary>
        ///     Save new or return existing cache matching parameters.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        ICache<TKey, TItem> Create<TKey, TItem>(Action<ICacheStrategyConfiguration> configure);
    }
}