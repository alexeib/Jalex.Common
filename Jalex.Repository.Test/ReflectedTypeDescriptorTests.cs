using FluentAssertions;
using Jalex.Repository.Utils;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Repository.Test
{
    public class ReflectedTypeDescriptorTests
    {
        private IFixture _fixture;

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
            string newId = obj.Id + "x";

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
    }
}
