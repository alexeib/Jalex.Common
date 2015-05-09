using Autofac;
using Jalex.Infrastructure.Configuration;
using Jalex.Infrastructure.ReflectedTypeDescriptor;

namespace Jalex.Infrastructure
{
    public class InfrastructureModule : Module
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
            builder.RegisterType<ReflectedTypeDescriptorProvider>()
                   .As<IReflectedTypeDescriptorProvider>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<ConfigurationProvider>()
                   .As<IConfigurationProvider>()
                   .InstancePerLifetimeScope();
        }

        #endregion
    }
}
