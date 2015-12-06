using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using Jalex.Caching.Memory;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
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
            _fixture.Register<IIdProvider>(_fixture.Create<GuidIdProvider>);
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);
            _fixture.Register<IQueryableRepository<TestEntity>>(_fixture.Create<MemoryRepository<TestEntity>>);

            registerCache();
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

            repo.SaveManyAsync(entities, WriteMode.Insert).Wait();

            TestEntity retrieved = cacheResponsibility.GetByIdAsync(entities.First().Id).Result;

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

            repo.SaveAsync(e2, WriteMode.Insert).Wait();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            cacheResponsibility.GetByIdAsync(e1.Id).Result.ShouldBeEquivalentTo(e1);
            cacheResponsibility.GetByIdAsync(e2.Id).Result.ShouldBeEquivalentTo(e2);
            cacheResponsibility.GetByIdAsync(e3.Id).Result.ShouldBeEquivalentTo(e3);
        }

        [Fact]
        public void GetAll_Populates_Cache()
        {
            var entities = _fixture.CreateMany<TestEntity>().ToArray();

            var cache = _fixture.Freeze<ICache<Guid, TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            var cacheResponsibility = _fixture.Create<CacheResponsibility<TestEntity>>();

            repo.SaveManyAsync(entities, WriteMode.Insert).Wait();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            cacheResponsibility.GetAllAsync().Result.ToArray();

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

            repo.SaveManyAsync(entities, WriteMode.Insert).Wait();

            var retrieved = cacheResponsibility.FirstOrDefaultAsync(e => e.Id == entities.First().Id).Result;

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

            repo.SaveManyAsync(entities, WriteMode.Insert).Wait();

            var retrieved = cacheResponsibility.QueryAsync(e => e.Id == entities.First().Id).Result.FirstOrDefault();

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

            repo.SaveManyAsync(entities, WriteMode.Insert).Wait();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            cacheResponsibility.GetAllAsync().Result.ToArray();

            var entityToDelete = entities.First();

            cacheResponsibility.DeleteAsync(entityToDelete.Id).Wait();

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

            cacheResponsibility.SaveAsync(entity, WriteMode.Insert).Wait();

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

            cacheResponsibility.SaveAsync(entity, WriteMode.Upsert).Wait();

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

            repo.SaveAsync(entity, WriteMode.Insert).Wait();

            entity.Name = _fixture.Create<string>();
            cacheResponsibility.SaveAsync(entity, WriteMode.Update).Wait();

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

            cacheResponsibility.SaveManyAsync(new[] { entity }, WriteMode.Insert).Wait();

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

            cacheResponsibility.SaveManyAsync(new[] { entity }, WriteMode.Upsert).Wait();

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

            repo.SaveAsync(entity, WriteMode.Insert).Wait();

            entity.Name = _fixture.Create<string>();
            cacheResponsibility.SaveManyAsync(new[] { entity }, WriteMode.Update).Wait();

            TestEntity retrieved;
            cache.TryGet(entity.Id, out retrieved).Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(entity);
        }
    }
}
