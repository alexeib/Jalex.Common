using System;

namespace Jalex.Infrastructure.Caching
{
    /// <summary>
    ///     CacheCapabilities defines ICacheFactory's implementation capabilites.
    /// </summary>
    [Flags]
    public enum CacheCapabilities
    {
        None = 0,
        InProcess = 0x1,
        Distributed = 0x2
    }
}