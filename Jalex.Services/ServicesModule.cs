using Autofac;
using Jalex.Infrastructure.Repository;
using Jalex.Services.Caching;

namespace Jalex.Services
{
    public class ServicesModule : Module
    {
        #region Overrides of Module

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<IndexCacheFactory>()
                .As<IIndexCacheFactory>()
                .InstancePerLifetimeScope();

            builder
                .RegisterGenericDecorator(typeof(CacheResponsibility<>),
                                             typeof(IQueryableRepository<>),
                                             fromKey: "repository")
                .InstancePerLifetimeScope();
        }

        #endregion
    }
}
