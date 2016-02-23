using System;
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

            _fixture.Register(() => new TypedInstanceContainer<IInterface>());
        }

        [Fact]
        public void AddsMetricsSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();

            var testObjects = new IInterface[]
                              {
                                  _fixture.Create<InterfaceImpl>(),
                                  _fixture.Create<InterfaceImpl2>()
                              };

            foreach (var obj in testObjects)
            {
                sut.Add(obj);
            }

            sut.Should().NotBeEmpty();
            sut.ShouldAllBeEquivalentTo(testObjects);
        }

        [Fact]
        public void AddsMetricsSuccessfullyUsingExtension()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();

            var testObjects = new IInterface[]
                              {
                                  _fixture.Create<InterfaceImpl>(),
                                  _fixture.Create<InterfaceImpl2>()
                              };

            sut.AddMany(testObjects);

            sut.Should().NotBeEmpty();
            sut.ShouldAllBeEquivalentTo(testObjects);
        }

        [Fact]
        public void GetsSingleInstanceSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();
            testObject.Id = string.Empty;

            sut.Add(testObject);

            var expected = sut.GetSingle<InterfaceImpl>();
            expected.Should().BeSameAs(testObject);
        }

        [Fact]
        public void GetsAllInstancesSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObjects = _fixture.CreateMany<InterfaceImpl>()
                                      .ToList();
            sut.AddMany(testObjects);

            var expected = sut.GetAll<InterfaceImpl>();
            expected.ShouldAllBeEquivalentTo(testObjects);
        }

        [Fact]
        public void AddsANewInstance()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObjectOriginal = _fixture.Create<InterfaceImpl>();
            var testObjectReplacement = _fixture.Create<InterfaceImpl>();

            testObjectOriginal.Id = string.Empty;
            testObjectReplacement.Id = string.Empty;

            sut.Add(testObjectOriginal);
            sut.Add(testObjectReplacement);

            sut.Should().HaveCount(2);

            var expected = sut.GetAll<InterfaceImpl>();
            expected.Should()
                    .ContainInOrder(testObjectOriginal, testObjectReplacement);
            sut.Invoking(s => s.GetSingle<InterfaceImpl>())
               .ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void RemovesInstancesSuccessfully()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObject = _fixture.Create<InterfaceImpl>();

            sut.Add(testObject);
            sut.Remove(testObject);

            sut.Should().BeEmpty();

            var expected = sut.GetSingle<InterfaceImpl>();
            expected.Should().BeNull();
        }

        [Fact]
        public void EnumeratesContainedInstances()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObjects = _fixture.CreateMany<InterfaceImpl>().ToList();

            sut.AddMany(testObjects);

            sut.Should().Contain(testObjects);
        }

        [Fact]
        public void CanBeRehydratedFromJson()
        {
            var sut = _fixture.Create<TypedInstanceContainer<IInterface>>();
            var testObjects = _fixture.CreateMany<InterfaceImpl>().ToList();

            sut.AddMany(testObjects);

            var str = sut.SerializeToString();

            var containerFromStr = new TypedInstanceContainer<IInterface>(str);

            containerFromStr.ShouldAllBeEquivalentTo(sut);
        }
    }
}
