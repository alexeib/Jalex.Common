using System.Linq;
using FluentAssertions;
using Jalex.Infrastructure.Containers;
using Jalex.Infrastructure.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Containers
{
    public class TypedInstanceContainerTests
    {
        private readonly IFixture _fixture;
        public TypedInstanceContainerTests()
        {
            _fixture = new Fixture();

            _fixture.Register(() => new TypedInstanceContainer<string, IInterface>(inst => inst.Id, string.Empty));
        }

        [Fact]
        public void AddsMetricsSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();

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
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();

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
        public void GetsDefaultInstanceSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();
            testObject.Id = string.Empty;

            sut.Set(testObject);

            var expected = sut.GetDefault<InterfaceImpl>();
            expected.Should().BeSameAs(testObject);
        }

        [Fact]
        public void GetsKeyedInstanceSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();

            sut.Set(testObject);

            var expected = sut.Get<InterfaceImpl>(testObject.Id);
            expected.Should().BeSameAs(testObject);
        }

        [Fact]
        public void ReplacesDefaultInstanceOfSameType()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObjectOriginal = _fixture.Create<InterfaceImpl>();
            var testObjectReplacement = _fixture.Create<InterfaceImpl>();

            testObjectOriginal.Id = string.Empty;
            testObjectReplacement.Id = string.Empty;

            sut.Set(testObjectOriginal);
            sut.Set(testObjectReplacement);

            sut.Should().HaveCount(1);

            var expected = sut.GetDefault<InterfaceImpl>();
            expected.Should().BeSameAs(testObjectReplacement);
        }

        [Fact]
        public void ReplacesKeyedInstanceOfSameType()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObjectOriginal = _fixture.Create<InterfaceImpl>();
            var testObjectReplacement = _fixture.Create<InterfaceImpl>();

            testObjectReplacement.Id = testObjectOriginal.Id;

            sut.Set(testObjectOriginal);
            sut.Set(testObjectReplacement);

            sut.Should().HaveCount(1);

            var expected = sut.Get<InterfaceImpl>(testObjectOriginal.Id);
            expected.Should().BeSameAs(testObjectReplacement);
        }

        [Fact]
        public void RemovesDefaultInstancesSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();
            testObject.Id = string.Empty;

            sut.Set(testObject);
            sut.RemoveDefault<InterfaceImpl>();

            sut.Should().BeEmpty();

            var expected = sut.GetDefault<InterfaceImpl>();
            expected.Should().BeNull();
        }

        [Fact]
        public void RemovesKeyedInstancesSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();

            sut.Set(testObject);
            sut.Remove<InterfaceImpl>(testObject.Id);

            sut.Should().BeEmpty();

            var expected = sut.Get<InterfaceImpl>(testObject.Id);
            expected.Should().BeNull();
        }

        [Fact]
        public void EnumeratesContainedInstances()
        {
            var sut = _fixture.Create<TypedInstanceContainer<string, IInterface>>();
            var testObjects = _fixture.CreateMany<InterfaceImpl>().ToList();

            sut.SetMany(testObjects);

            sut.Should().Contain(testObjects);
        }
    }
}
