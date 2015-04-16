using System.Runtime.InteropServices;
using Autofac;
using Autofac.Core;
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

            builder.RegisterGeneric(typeof (CacheResponsibility<>))
                   .WithParameter(new ResolvedParameter((ci, cc) => ci.ParameterType.IsGenericType && ci.ParameterType.GetGenericTypeDefinition() == typeof(IQueryableRepository<>),
                                                        (ci, cc) => cc.ResolveNamed("raw-repository", ci.ParameterType)))
                   .As(typeof(IReader<>), typeof(IWriter<>), typeof(IDeleter<>), typeof(ISimpleRepository<>), typeof(IQueryableRepository<>))
                   .Named("repository", typeof(IReader<>))
                   .Named("repository", typeof(IWriter<>))
                   .Named("repository", typeof(IDeleter<>))
                   .Named("repository", typeof(ISimpleRepository<>))
                   .Named("repository", typeof(IQueryableRepository<>))
                   .InstancePerLifetimeScope();
        }

        #endregion
    }
}
