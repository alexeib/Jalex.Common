using System.Collections.Generic;
using FluentAssertions;
using Jalex.Infrastructure.Configuration;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Configuration
{
    public class ConfigurationProviderTests
    {
        private readonly IFixture _fixture;
        private readonly IConfiguration _configuration1, _configuration2;

        public ConfigurationProviderTests()
        {
            _fixture = new Fixture();

            _configuration1 = _fixture.Create<Configuration1>();
            var overwrittenConfiguration2 = _fixture.Create<Configuration2>();
            _configuration2 = _fixture.Create<Configuration2>();

            var configurationSupplier1Sub = Substitute.For<IConfigurationSupplier>();
            configurationSupplier1Sub.GetConfigurations()
                                     .Returns(new[] { _configuration1, overwrittenConfiguration2 });
            var configurationSupplier2Sub = Substitute.For<IConfigurationSupplier>();
            configurationSupplier2Sub.GetConfigurations()
                                     .Returns(new[] {_configuration2});

            _fixture.Register<IEnumerable<IConfigurationSupplier>>(() => new[] {configurationSupplier1Sub, configurationSupplier2Sub});

            _fixture.Register<IConfigurationProvider>(() => _fixture.Create<ConfigurationProvider>());
        }

        [Fact]
        public void Returns_Correct_Configuration1()
        {
            var sut = _fixture.Create<IConfigurationProvider>();
            var conf = sut.GetConfiguration<Configuration1>();
            conf.Should()
                .Be(_configuration1);
        }

        [Fact]
        public void Returns_Correct_Configuration2()
        {
            var sut = _fixture.Create<IConfigurationProvider>();
            var conf = sut.GetConfiguration<Configuration2>();
            conf.Should()
                .Be(_configuration2);
        }

        [Fact]
        public void Does_Not_Return_Configuration3()
        {
            var sut = _fixture.Create<IConfigurationProvider>();
            var conf = sut.GetConfiguration<Configuration3>();
            conf.Should()
                .BeNull();
        }
    }
}
