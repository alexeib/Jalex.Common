using System;
using System.Collections.Generic;
using Jalex.Infrastructure.Containers;

namespace Jalex.Infrastructure.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly Lazy<TypedInstanceContainer<IConfiguration>> _configurationsContainer;

        public ConfigurationProvider(Lazy<IEnumerable<IConfigurationSupplier>> configurationSuppliers)
        {
            _configurationsContainer = new Lazy<TypedInstanceContainer<IConfiguration>>(() =>
                                                                                        {
                                                                                            var container = new TypedInstanceContainer<IConfiguration>();
                                                                                            foreach (var supplier in configurationSuppliers.Value)
                                                                                            {
                                                                                                var configurations = supplier.GetConfigurations();
                                                                                                container.AddMany(configurations);
                                                                                            }
                                                                                            return container;
                                                                                        });
        }

        #region Implementation of IConfigurationProvider

        public TConfiguration GetConfiguration<TConfiguration>() where TConfiguration : class, IConfiguration
        {
            var configuration = _configurationsContainer.Value.GetSingle<TConfiguration>();
            if (configuration == null)
            {
                throw new ConfigurationMissingException(typeof(TConfiguration));
            }
            return configuration;
        }

        #endregion
    }
}
