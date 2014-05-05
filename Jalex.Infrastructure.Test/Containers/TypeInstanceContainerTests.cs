using FluentAssertions;
using Jalex.Infrastructure.Containers;
using Jalex.Infrastructure.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Containers
{
    public class TypeInstanceContainerTests
    {
        private readonly IFixture _fixture;
        public TypeInstanceContainerTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void AddsMetricsSuccessfully()
        {
            var sut = _fixture.Create<TypeInstanceContainer<IInterface>>();

            var testObjects = new IInterface[]
                              {
                                  _fixture.Create<InterfaceImpl>(),
                                  _fixture.Create<InterfaceImpl2>()
                              };

            foreach (var obj in testObjects)
            {
                sut.Set(obj);
            }

            sut.Should().NotBeEmpty();
            sut.ShouldAllBeEquivalentTo(testObjects);
        }

        [Fact]
        public void AddsMetricsSuccessfullyUsingExtension()
        {
            var sut = _fixture.Create<TypeInstanceContainer<IInterface>>();

            var testObjects = new IInterface[]
                              {
                                  _fixture.Create<InterfaceImpl>(),
                                  _fixture.Create<InterfaceImpl2>()
                              };

            sut.SetMany(testObjects);

            sut.Should().NotBeEmpty();
            sut.ShouldAllBeEquivalentTo(testObjects);
        }

        [Fact]
        public void GetsMetricsSuccessfully()
        {
            var sut = _fixture.Create<TypeInstanceContainer<IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();

            sut.Set(testObject);

            var expected = sut.Get<InterfaceImpl>();
            expected.Should().BeSameAs(testObject);
        }

        [Fact]
        public void ReplacesMetricOfSameType()
        {
            var sut = _fixture.Create<TypeInstanceContainer<IInterface>>();
            var testObjectOriginal = _fixture.Create<InterfaceImpl>();
            var testObjectReplacement = _fixture.Create<InterfaceImpl>();

            sut.Set(testObjectOriginal);
            sut.Set(testObjectReplacement);

            sut.Should().HaveCount(1);

            var expected = sut.Get<InterfaceImpl>();
            expected.Should().BeSameAs(testObjectReplacement);
        }

        [Fact]
        public void RemovesMetricsSuccessfully()
        {
            var sut = _fixture.Create<TypeInstanceContainer<IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();

            sut.Set(testObject);
            sut.Remove<InterfaceImpl>();

            sut.Should().BeEmpty();

            var expected = sut.Get<InterfaceImpl>();
            expected.Should().BeNull();
        }
    }
}
