using System;
using Autofac;
using Jalex.Caching.Memory;
using Jalex.Caching.NoOp;
using Jalex.Infrastructure.Caching;

namespace Jalex.Caching
{
    public class CacheModule : Module
    {
        public CacheType CacheType { get; set; }

        #region Overrides of Module

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<Action<ICacheStrategyConfiguration>>(context => (conf) => conf.UseTypedScope())
                .SingleInstance();

            switch (CacheType)
            {
                case CacheType.NoOp:
                    builder.RegisterType<NoOpCacheFactory>()
                                       .As<ICacheFactory>()
                                       .InstancePerLifetimeScope();                    

                    break;
                case CacheType.Memory:
                    builder.RegisterType<MemoryCacheFactory>()
                                       .As<ICacheFactory>()
                                       .InstancePerLifetimeScope();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Cache type " + CacheType + " is not supported");
            }
        }

        #endregion
    }
}
