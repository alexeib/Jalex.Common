using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Jalex.Infrastructure.Extensions;
using Magnum.Extensions;
using MoreLinq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Jalex.Infrastructure.Configuration
{
    public class YamlConfigurationSupplier : IConfigurationSupplier
    {
        private static readonly string[] _defaultConfigFileNames =
        {
            $"{AppDomain.CurrentDomain.FriendlyName.Replace("vshost.", string.Empty)}.yaml",
            "config.yaml"
        };

        private readonly IEnumerable<string> _configFileNames;

        public YamlConfigurationSupplier()
            : this(null)
        {
        }

        public YamlConfigurationSupplier(IEnumerable<string> configFileNames)
        {
            _configFileNames = configFileNames ?? _defaultConfigFileNames;
        }

        #region Implementation of IConfigurationSupplier

        public IEnumerable<IConfiguration> GetConfigurations()
        {
            var configFileName = _configFileNames.FirstOrDefault(File.Exists);

            if (configFileName.IsNullOrEmpty())
            {
                yield break;
            }

            IDictionary<string, Type> configurationTypesByName;

            try
            {
                var configurationTypes =
                    from a in AppDomain.CurrentDomain.GetAssemblies()
                    from t in a.GetTypes()
                    where t.IsClass && t.Implements(typeof (IConfiguration))
                    select t;
                configurationTypesByName = configurationTypes.ToUniqueDictionary(t => t.Name);
            }
            catch (ReflectionTypeLoadException loadException)
            {
                throw new AggregateException(loadException.Concat(loadException.LoaderExceptions));
            }


            var yamlDeserializer = new Deserializer();
            var fileContents = File.ReadAllText(configFileName);

            using (var reader = new StringReader(fileContents))
            {
                var parser = new Parser(reader);
                var yamlReader = new EventReader(parser);

                yamlReader.Expect<StreamStart>();
                yamlReader.Expect<DocumentStart>();
                yamlReader.Expect<MappingStart>();

                var currScalar = yamlReader.Allow<Scalar>();
                while (currScalar != null)
                {
                    var configTypeName = currScalar.Value;

                    Type configType;
                    if (!configurationTypesByName.TryGetValue(configTypeName, out configType))
                    {
                        yamlReader.SkipThisAndNestedEvents();
                        currScalar = yamlReader.Allow<Scalar>();
                        continue;
                    }

                    var configuration = yamlDeserializer.Deserialize(yamlReader, configType) as IConfiguration;
                    if (configuration == null)
                    {
                        throw new YamlConfigurationException($"Could not deserialize type {configTypeName}");
                    }

                    yield return configuration;

                    currScalar = yamlReader.Allow<Scalar>();
                }
            }
        }

        #endregion
    }
}
