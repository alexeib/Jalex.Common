using System;
using FluentAssertions;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.ReflectedTypeDescriptor
{
    public class ReflectedTypeDescriptorTests
    {
        private readonly IFixture _fixture;

        public ReflectedTypeDescriptorTests()
        {
            _fixture = new Fixture();
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);
        }

        [Fact]
        public void Caches_Reflected_Type_Descriptor()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();

            var typeDescriptor = provider.GetReflectedTypeDescriptor<TestObject>();
            typeDescriptor.Should().NotBeNull();
            var typeDescriptor2 = provider.GetReflectedTypeDescriptor<TestObject>();
            typeDescriptor2.Should().NotBeNull();
            typeDescriptor2.Should().BeSameAs(typeDescriptor);
        }

        [Fact]
        public void Gets_The_Right_Id()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();

            var obj = _fixture.Create<TestObject>();
            var sut = provider.GetReflectedTypeDescriptor<TestObject>();

            var id = sut.GetId(obj);
            id.Should().Be(obj.Id);
        }

        [Fact]
        public void Sets_Id()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();

            var obj = _fixture.Create<TestObject>();
            Guid newId = _fixture.Create<Guid>();

            var sut = provider.GetReflectedTypeDescriptor<TestObject>();

            sut.SetId(obj, newId);
            obj.Id.Should().Be(newId);
        }

        [Fact]
        public void Sets_Id_Property_Name_To_Correct_Value()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();
            var sut = provider.GetReflectedTypeDescriptor<TestObject>();
            sut.IdPropertyName.Should().Be("Id");
        }

        [Fact]
        public void Gets_Value_Of_Property_By_Name()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();
            
            var obj = _fixture.Create<TestObject>();
            var sut = provider.GetReflectedTypeDescriptor<TestObject>();

            var propValue = sut.GetPropertyValue("RefId", obj);
            propValue.Should().Be(obj.RefId);
        }

        [Fact]
        public void Gets_Value_Of_Property_By_Name_If_Prop_Is_Number()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();

            var obj = _fixture.Create<TestObject>();
            var sut = provider.GetReflectedTypeDescriptor<TestObject>();

            var propValue = sut.GetPropertyValue("Number", obj);
            propValue.Should().Be(obj.Number);
        }

        [Fact]
        public void Throws_On_Invalid_Property()
        {
            var provider = _fixture.Create<IReflectedTypeDescriptorProvider>();

            var obj = _fixture.Create<TestObject>();
            var propName = _fixture.Create<string>();
            var sut = provider.GetReflectedTypeDescriptor<TestObject>();

            sut.Invoking(s => s.GetPropertyValue(propName, obj)).ShouldThrow<ArgumentException>();
        }
    }
}
