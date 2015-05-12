using System.Linq;
using FluentAssertions;
using Jalex.Infrastructure.Configuration;
using Jalex.Infrastructure.Extensions;
using Jalex.TestUtils.xUnit;
using Xunit;

namespace Jalex.Infrastructure.Test.Configuration
{
    public class YamlConfigurationSupplierTest
    {
        private readonly string _configLocation = XUnitUtils.GetDeployedFileLocation(@"Configuration\config.yaml");

        [Fact]
        public void Deserializes_Configuration_Correctly()
        {
            var configSupplier = new YamlConfigurationSupplier(new[] {_configLocation});
            var configs = configSupplier.GetConfigurations()
                                        .ToCollection();
            configs.Should()
                   .NotBeNullOrEmpty()
                   .And.HaveCount(2);

            var config1 = configs.OfType<Configuration1>().FirstOrDefault();
            config1.Should()
                   .NotBeNull();
            config1.SomeProp.Should()
                   .Be("prop value");

            configs.OfType<Configuration2>().FirstOrDefault().Should().BeNull();

            var config3 = configs.OfType<Configuration3>().FirstOrDefault();
            config3.Should()
                   .NotBeNull();
            config3.Str.Should()
                   .Be("str value");
        }
    }
}
