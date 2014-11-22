using System.Reflection;
using FluentAssertions;
using Jalex.Infrastructure.Serialization;
using Jalex.Infrastructure.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Serialization
{
    public class CustomTypeNameBinderTests
    {
        private readonly IFixture _fixture;

        public CustomTypeNameBinderTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void BindsToNameOfCustomType()
        {
            var sut = _fixture.Create<CustomTypeNameBinder>();
            var inst = _fixture.Create<CustomTypeNamedInterface>();

            var type = inst.GetType();
            var customNameAttribute = type.GetCustomAttribute<CustomTypeNameAttribute>();
            string assemblyName, typeName;

            sut.BindToName(type, out assemblyName, out typeName);

            assemblyName.Should().BeNull();
            typeName.Should().Be(customNameAttribute.CustomTypeName);
        }

        [Fact]
        public void BindsToNameOfNonCustomType()
        {
            var sut = _fixture.Create<CustomTypeNameBinder>();
            var nonCustomType = _fixture.Create<InterfaceImpl>();

            var type = nonCustomType.GetType();
            string assemblyName, typeName;

            sut.BindToName(type, out assemblyName, out typeName);

            assemblyName.Should().BeNull();
            typeName.Should().Be(type.FullName);
        }

        [Fact]
        public void BindsToTypeWithCustomName()
        {
            var sut = _fixture.Create<CustomTypeNameBinder>();
            var inst = _fixture.Create<CustomTypeNamedInterface>();
            var instType = inst.GetType();
            var customNameAttribute = instType.GetCustomAttribute<CustomTypeNameAttribute>();

            var type = sut.BindToType(instType.Assembly.FullName, customNameAttribute.CustomTypeName);

            type.Should().Be(instType);
        }

        [Fact]
        public void BindsToTypeWithoutCustomName()
        {
            var sut = _fixture.Create<CustomTypeNameBinder>();
            var inst = _fixture.Create<InterfaceImpl>();
            var instType = inst.GetType();

            var type = sut.BindToType(instType.Assembly.FullName, instType.FullName);

            type.Should().Be(instType);
        }
    }
}
