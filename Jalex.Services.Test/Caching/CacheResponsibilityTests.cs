using System;
using System.Linq;
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
    public class CacheResponsibilityTests
    {
        private readonly IFixture _fixture;

        public CacheResponsibilityTests()
        {
            _fixture = new Fixture();            

            _fixture.Register(() => _fixture.Build<TestEntity>().With(e => e.Id, Guid.NewGuid()).Create());
            _fixture.Register<ILogger>(_fixture.Create<MemoryLogger>);
            _fixture.Register<IIdProvider>(_fixture.Create<GuidIdProvider>);
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);
            _fixture.Register<IQueryableRepository<TestEntity>>(_fixture.Create<MemoryRepository<TestEntity>>);

            registerCache();
            registerIndexCache();
        }

        private void registerIndexCache()
        {
            _fixture.Register<IIndexCache<TestEntity>>(() => new IndexCache<TestEntity>(
                new[] { "ClusteredKey", "ClusteredKey2" },
                _fixture.Create<ICacheFactory>(),
                _fixture.Create<Action<ICacheStrategyConfiguration>>(),
                _fixture.Create<IReflectedTypeDescriptorProvider>()));
            var indexCache = _fixture.Freeze<IIndexCache<TestEntity>>();

            var indexCacheFactory = Substitute.For<IIndexCacheFactory>();
            indexCacheFactory
                .CreateIndexCachesForType<TestEntity>()
                .Returns(new[] { indexCache });
            _fixture.Inject(indexCacheFactory);
        }

        private void registerCache()
        {
            _fixture.Register<ICache<Guid, TestEntity>>(_fixture.Create<MemoryCache<Guid, TestEntity>>);
            _fixture.Register<ICache<string, Guid>>(_fixture.Create<MemoryCache<string, Guid>>);

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var cacheForIndex = _fixture.Freeze<ICache<string, Guid>>();

            var cacheFactory = Substitute.For<ICacheFactory>();
            cacheFactory.Create<Guid, TestEntity>(null).ReturnsForAnyArgs(cache);
            cacheFactory.Create<string, Guid>(null).ReturnsForAnyArgs(cacheForIndex);
            _fixture.Inject(cacheFactory);
        }

        [Fact]
        public void Get_Populates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveMany(entities, WriteMode.Insert);

            TestEntity retrieved;
            var success = cacheResponsibility.TryGetById(entities.First().Id, out retrieved);

            success.Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entities.First());

            cache
                .GetAll()
                .Should()
                .ContainSingle(kvp => kvp.Key == retrieved.Id && kvp.Value.IsSameOrEqualTo(retrieved));
        }

        [Fact]
        public void Get_Uses_Cache()
        {
            var e1 = _fixture.Create<TestEntity>();
            var e2 = _fixture.Create<TestEntity>();
            var e3 = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            cache.Set(e1.Id, e1);
            cache.Set(e3.Id, e3);

            repo.Save(e2, WriteMode.Insert);

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.GetByIdOrDefault(e1.Id).ShouldBeEquivalentTo(e1);
            cacheResponsibility.GetByIdOrDefault(e2.Id).ShouldBeEquivalentTo(e2);
            cacheResponsibility.GetByIdOrDefault(e3.Id).ShouldBeEquivalentTo(e3);
        }

        [Fact]
        public void GetAll_Populates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveMany(entities, WriteMode.Insert);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            cacheResponsibility.GetAll().ToArray();

            cache
                .GetAll()
                .Select(kvp => kvp.Value)
                .Should()
                .Contain(entities);
        }

        [Fact]
        public void FirstOrDefault_Populates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveMany(entities, WriteMode.Insert);

            var retrieved = cacheResponsibility.FirstOrDefault(e => e.Id == entities.First().Id);

            retrieved.ShouldBeEquivalentTo(entities.First());

            cache
                .GetAll()
                .Should()
                .ContainSingle(kvp => kvp.Key == retrieved.Id && kvp.Value.IsSameOrEqualTo(retrieved));
        }

        [Fact]
        public void Query_Populates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveMany(entities, WriteMode.Insert);

            var retrieved = cacheResponsibility.Query(e => e.Id == entities.First().Id).FirstOrDefault();

            retrieved.ShouldBeEquivalentTo(entities.First());

            cache
                .GetAll()
                .Should()
                .ContainSingle(kvp => kvp.Key == retrieved.Id && kvp.Value.IsSameOrEqualTo(retrieved));
        }

        [Fact]
        public void Delete_Invalidates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveMany(entities, WriteMode.Insert);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            cacheResponsibility.GetAll().ToArray();

            var entityToDelete = entities.First();

            cacheResponsibility.Delete(entityToDelete.Id);

            TestEntity entity;
            cache.TryGet(entityToDelete.Id, out entity).Should().BeFalse();
            entity.Should().BeNull();
        }

        [Fact]
        public void Save_Single_With_Insert_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.Save(entity, WriteMode.Insert);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public void Save_Single_With_Upsert_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.Save(entity, WriteMode.Upsert);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public void Save_Single_With_Update_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.Save(entity, WriteMode.Insert);

            entity.Name = _fixture.Create<string>();
            cacheResponsibility.Save(entity, WriteMode.Update);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public void Save_Many_With_Insert_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.SaveMany(new[] { entity }, WriteMode.Insert);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public void Save_Many_With_Upsert_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.SaveMany(new[] { entity }, WriteMode.Upsert);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public void Save_Many_With_Update_Updates_Cache()
        {
            var entity = _fixture.Create<TestEntity>();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.Save(entity, WriteMode.Insert);

            entity.Name = _fixture.Create<string>();
            cacheResponsibility.SaveMany(new[] { entity }, WriteMode.Update);

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }
    }
}
