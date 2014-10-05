using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Jalex.Infrastructure.Utils;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Infrastructure.Test.Utils
{
    public class CollectionUtilsTests
    {
        private readonly IFixture _fixture;

        public CollectionUtilsTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Throws_If_Collection_Is_Null()
        {
            Action invocation = () => CollectionUtils.LockAndCreateNewCollectionWithItemAppended(new object(), null, new object());
            invocation.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Throws_If_LockObject_Is_Null()
        {
            Action invocation = () => CollectionUtils.LockAndCreateNewCollectionWithItemAppended(new object(), new object[0], null);
            invocation.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Creates_New_Collection_With_Item_Appended()
        {
            var itemToAdd = _fixture.Create<object>();
            var collection = _fixture.CreateMany<object>();
            var lockObject = _fixture.Create<object>();

            var result = CollectionUtils.LockAndCreateNewCollectionWithItemAppended(itemToAdd, collection, lockObject);

            result
                .Should()
                .NotBeNull()
                .And
                .ContainInOrder(collection.Concat(new[] {itemToAdd}));
        }
    }
}
