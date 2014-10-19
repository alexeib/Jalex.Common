﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching.Memory
{
    /// <summary>
    ///     MemCacheFactory implementation of cache factory.
    /// </summary>
    /// <remarks>
    ///     MemCacheFactory will hold on and reuse all the instances returned via its Save factory method.
    /// </remarks>
    public class MemoryCacheFactory : ICacheFactory
    {
        private readonly ConcurrentDictionary<string, object> _caches = new ConcurrentDictionary<string, object>();
        private bool _isDisposed;

        /// <summary>
        ///     Creates or gets a new cache factory for a given entity type.
        /// </summary>
        public virtual ICache<TKey, TEntity> Create<TKey, TEntity>(Action<ICacheStrategyConfiguration> configure)
            where TEntity : class
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);            

            var conf = new MemoryCacheConfiguration();
            configure(conf);

            if (string.IsNullOrEmpty(conf.CacheName))
            {
                throw new InvalidOperationException("Cache name cannot be empty");
            }

            StringBuilder cacheNameBuilder = new StringBuilder(conf.CacheName);

            if (!string.IsNullOrEmpty(conf.NamedScope))
            {
                cacheNameBuilder.AppendFormat(".{0}", conf.NamedScope);
            }

            if (conf.IsUsingTypedScope)
            {
                cacheNameBuilder.AppendFormat(".{0}", typeof(TEntity).FullName);
            }

            var key = cacheNameBuilder.ToString();

            return _caches.GetOrAdd(key, new MemoryCache<TKey, TEntity>()) as MemoryCache<TKey, TEntity>;
        }

        public IEnumerable<string> GetCacheNames()
        {
            return _caches.Keys;
        }

        public CacheCapabilities GetCapabilities()
        {
            return CacheCapabilities.InProcess;
        }

        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                foreach (IDisposable cache in _caches.Values.Where(m => m is IDisposable))
                    cache.Dispose();

                _caches.Clear();

                _isDisposed = true;
            }
        }        
    }
}