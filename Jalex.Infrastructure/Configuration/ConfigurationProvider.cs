using System;
using System.Collections.Generic;
using Jalex.Infrastructure.Containers;

namespace Jalex.Infrastructure.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly Lazy<TypedInstanceContainer<string, IConfiguration>> _configurationsContainer;

        public ConfigurationProvider(Lazy<IEnumerable<IConfigurationSupplier>> configurationSuppliers)
        {
            _configurationsContainer = new Lazy<TypedInstanceContainer<string, IConfiguration>>(() =>
                                                                                                {
                                                                                                    var container =
                                                                                                        new TypedInstanceContainer<string, IConfiguration>(c => c.GetType()
                                                                                                                                                                 .FullName,
                                                                                                                                                           string.Empty);
                                                                                                    foreach (var supplier in configurationSuppliers.Value)
                                                                                                    {
                                                                                                        var configurations = supplier.GetConfigurations();
                                                                                                        foreach (var configuration in configurations)
                                                                                                        {
                                                                                                            container.SetDefault(configuration);
                                                                                                        }
                                                                                                    }
                                                                                                    return container;
                                                                                                });
        }

        #region Implementation of IConfigurationProvider

        public TConfiguration GetConfiguration<TConfiguration>() where TConfiguration : class, IConfiguration
        {
            var configuration = _configurationsContainer.Value.GetDefault<TConfiguration>();
            if (configuration == null)
            {
                throw new ConfigurationMissingException(typeof(TConfiguration));
            }
            return configuration;
        }

        #endregion
    }
}
