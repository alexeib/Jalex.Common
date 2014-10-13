using System;
using System.Runtime.Remoting.Proxies;
using FluentAssertions;
using FluentAssertions.Common;
using Jalex.Caching.Memory;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Logging.Loggers;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Services.Caching;
using Jalex.Services.Test.Fixtures;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Services.Test.Caching
{
    public class IndexCacheTests
    {
        private readonly IFixture _fixture;

        public IndexCacheTests()
        {
            _fixture = new Fixture();

            registerCache();

            _fixture.Register<ILogger>(_fixture.Create<MemoryLogger>);
            _fixture.Register<IIdProvider>(_fixture.Create<GuidIdProvider>);
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);
            _fixture.Register<IQueryableRepository<TestEntity>>(_fixture.Create<MemoryRepository<TestEntity>>);
        }

        private void registerCache()
        {
            _fixture.Register<ICache<string, string>>(_fixture.Create<MemoryCache<string, string>>);
            var cache = _fixture.Freeze<ICache<string, string>>();

            var cacheFactory = Substitute.For<ICacheFactory>();
            cacheFactory.Create<string, string>(null).ReturnsForAnyArgs(cache);
            _fixture.Inject(cacheFactory);
        }

        [Fact]
        public void Indexed_Properties_Contain_Indexed_Properties()
        {
            var indexedProps = new[] {"ClusteredKey", "ClusteredKey2"};

            var indexCache = new IndexCache<TestEntity>(
                indexedProps,
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache
                .IndexedProperties
                .ShouldBeEquivalentTo(indexedProps);
        }

        [Fact]
        public void Index_Adds_To_Cache()
        {
            var entity = _fixture.Create<TestEntity>();
            var cache = _fixture.Freeze<ICache<string, string>>();
            var indexCache = new IndexCache<TestEntity>(
                new[] {"ClusteredKey", "ClusteredKey2"},
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);

            cache
                .GetAll()
                .Should()
                .ContainSingle(kvp => kvp.Value.IsSameOrEqualTo(entity.Id));
        }

        [Fact]
        public void DeIndex_Removes_From_Cache()
        {
            var entity = _fixture.Create<TestEntity>();
            var cache = _fixture.Freeze<ICache<string, string>>();
            var indexCache = new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);
            indexCache.DeIndex(entity);

            cache
                .GetAll()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void DeIndex_By_Query_Removes_From_Cache()
        {
            var entity = _fixture.Create<TestEntity>();
            var cache = _fixture.Freeze<ICache<string, string>>();
            var indexCache = new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            string clusteredKeyVal = entity.ClusteredKey, clusteredKeyVal2 = entity.ClusteredKey2;

            indexCache.Index(entity);
            indexCache.DeIndexByQuery(e => e.ClusteredKey == clusteredKeyVal && e.ClusteredKey2 == clusteredKeyVal2);

            cache
                .GetAll()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void FindIdByQuery_Retrieves_Indexed_Entity()
        {
            var entity = _fixture.Create<TestEntity>();
            
            var indexCache = new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);

            string clusteredKeyVal = entity.ClusteredKey, clusteredKeyVal2 = entity.ClusteredKey2;

            indexCache
                .FindIdByQuery(e => e.ClusteredKey == clusteredKeyVal && e.ClusteredKey2 == clusteredKeyVal2)
                .ShouldBeEquivalentTo(entity.Id);
        }

        [Fact]
        public void FindIdByQuery_Returns_Null_For_Non_Indexed_Entity()
        {
            var entity = _fixture.Create<TestEntity>();
            var indexCache = new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);

            indexCache
                .FindIdByQuery(e => e.ClusteredKey == _fixture.Create<string>() && e.ClusteredKey2 == entity.ClusteredKey2)
                .Should()
                .BeNull();
        }

        [Fact]
        public void FindIdByQuery_Returns_Null_When_Not_Using_Equality_Operators()
        {
            var entity = _fixture.Create<TestEntity>();

            var indexCache = new IndexCache<TestEntity>(
                new[] { "NumValue" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);

            indexCache
                .FindIdByQuery(e => e.NumValue >= entity.NumValue)
                .Should()
                .BeNull();
        }

        [Fact]
        public void FindIdByQuery_Retrieves_Indexed_Entity_With_Comlex_Constant()
        {
            var entity = _fixture.Create<TestEntity>();

            var indexCache = new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>());

            indexCache.Index(entity);

            string clusteredKeyVal = entity.ClusteredKey, clusteredKeyVal2 = entity.ClusteredKey2;

            indexCache
                .FindIdByQuery(e => e.ClusteredKey == clusteredKeyVal.Substring(0, clusteredKeyVal.Length) + clusteredKeyVal.Substring(clusteredKeyVal.Length) && e.ClusteredKey2 == clusteredKeyVal2)
                .ShouldBeEquivalentTo(entity.Id);
        }
    }
}
