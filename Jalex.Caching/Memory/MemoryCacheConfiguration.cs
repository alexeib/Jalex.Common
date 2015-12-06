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

        public ICacheStrategyConfiguration UseNamedScope(string name)
        {
            NamedScope = name;
            return this;
        }

        public ICacheStrategyConfiguration UseTypedScope()
        {
            IsUsingTypedScope = true;
            return this;
        }
    }
}
