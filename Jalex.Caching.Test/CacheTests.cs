using System.Linq;
using FluentAssertions;
using Jalex.Caching.Test.Fixtures;
using Jalex.Infrastructure.Caching;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Caching.Test
{
    public abstract class CacheTests
    {
        private readonly IFixture _fixture;

        protected CacheTests(IFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Returns_Cached_Item()
        {
            var item = _fixture.Create<TestEntity>();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            sut.Set(item.Id, item);

            var retrievedItem = sut.Get(item.Id);

            retrievedItem.ShouldBeEquivalentTo(item);
        }

        [Fact]
        public void Returns_Null_For_Uncached_Item()
        {
            var item = _fixture.Create<TestEntity>();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            var retrievedItem = sut.Get(item.Id);

            retrievedItem.Should().BeNull();
        }

        [Fact]
        public void Returns_Null_For_Deleted_Item()
        {
            var item = _fixture.Create<TestEntity>();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            sut.Set(item.Id, item);
            sut.DeleteById(item.Id);

            var retrievedItem = sut.Get(item.Id);

            retrievedItem.Should().BeNull();
        }

        [Fact]
        public void Returns_Null_After_Deleting_Everything()
        {
            var item = _fixture.Create<TestEntity>();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            sut.Set(item.Id, item);
            sut.DeleteAll().Wait();

            var retrievedItem = sut.Get(item.Id);

            retrievedItem.Should().BeNull();
        }

        [Fact]
        public void Gets_Many_Items_With_Partial_Fill()
        {
            var item = _fixture.Create<TestEntity>();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            sut.Set(item.Id, item);

            var randomIds = _fixture.CreateMany<string>();
            var args = randomIds.Concat(new[] { item.Id });

            var retrievedItems = sut.GetMany(args).ToArray();

            retrievedItems
                .Should()
                .HaveCount(1);

            var firstItem = retrievedItems.First();

            firstItem.Key.Should().Be(item.Id);
            firstItem.Value.ShouldBeEquivalentTo(item);

        }

        [Fact]
        public void Gets_All_Retrieves_All_Items()
        {
            var items = _fixture.CreateMany<TestEntity>().ToArray();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            foreach (var item in items)
            {
                sut.Set(item.Id, item);
            }

            var retrievedItems = sut.GetAll().ToArray();

            retrievedItems
                .Should()
                .HaveCount(items.Length);

            foreach (var item in items)
            {
                var cachedItem = retrievedItems.FirstOrDefault(kvp => kvp.Key == item.Id);
                cachedItem.Should().NotBeNull();
                cachedItem.Key.Should().Be(item.Id);
                cachedItem.Value.ShouldBeEquivalentTo(item);
            }
        }

        [Fact]
        public void Gets_Keys_Retrieves_Keys_Of_All_Items()
        {
            var items = _fixture.CreateMany<TestEntity>().ToArray();
            var sut = _fixture.Create<ICache<string, TestEntity>>();

            foreach (var item in items)
            {
                sut.Set(item.Id, item);
            }

            var keys = sut.GetKeys().ToArray();

            keys
                .Should()
                .HaveCount(items.Length)
                .And
                .Contain(items.Select(i => i.Id));
        }
    }
}