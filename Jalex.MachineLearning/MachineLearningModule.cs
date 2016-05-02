using Autofac;

namespace Jalex.MachineLearning
{
    public class MachineLearningModule : Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(typeof(MachineLearningModule).Assembly)
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
        }

        #endregion
    }
}
