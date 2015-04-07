using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Jalex.Caching.Memory;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Logging.Loggers;
using Jalex.Repository.IdProviders;
using Jalex.Services.Caching;
using Jalex.Services.Test.Fixtures;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Services.Test.Caching
{
    public class IndexCacheResponsibilityTests
    {
        private readonly IFixture _fixture;

        public IndexCacheResponsibilityTests()
        {
            _fixture = new Fixture();

            _fixture.Register(() => _fixture.Build<TestEntity>().With(e => e.Id, Guid.NewGuid()).Create());
            _fixture.Register<ILogger>(_fixture.Create<MemoryLogger>);
            _fixture.Register<IIdProvider>(_fixture.Create<GuidIdProvider>);
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);

            registerRepository();
            registerCache();
            registerIndexCache();
        }

        private void registerRepository()
        {
            var repository = Substitute.For<IQueryableRepository<TestEntity>>();
            _fixture.Inject(repository);
        }

        private void registerCache()
        {
            _fixture.Register<ICache<string, Guid>>(_fixture.Create<MemoryCache<string, Guid>>);

            var cacheForIndex = _fixture.Freeze<ICache<string, Guid>>();

            var cacheFactory = Substitute.For<ICacheFactory>();
            cacheFactory.Create<string, Guid>(null).ReturnsForAnyArgs(cacheForIndex);
            _fixture.Inject(cacheFactory);
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

        [Fact]
        public void FirstOrDefault_Uses_Cache()
        {
            var e1 = _fixture.Create<TestEntity>();
            var e2 = _fixture.Create<TestEntity>();

            var indexCache = _fixture.Freeze<IIndexCache<TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            TestEntity dummy1;
            repo
                .TryGetById(e1.Id, out dummy1)
                .Returns(ci =>
                         {
                             ci[1] = e1;
                             return true;
                         });

            repo
                .FirstOrDefault(Arg.Any<Expression<Func<TestEntity, bool>>>())
                .ReturnsForAnyArgs(ci =>
                         {
                             var q = ci.Arg<Expression<Func<TestEntity, bool>>>().Compile();
                             return q(e2) ? e2 : null;
                         });

            indexCache.Index(e1);

            var indexCacheResponsibility = _fixture.Create<IndexCacheResponsibility<TestEntity>>();

            indexCacheResponsibility.FirstOrDefault(e => e.ClusteredKey == e1.ClusteredKey && e.ClusteredKey2 == e1.ClusteredKey2).ShouldBeEquivalentTo(e1);
            indexCacheResponsibility.FirstOrDefault(e => e.ClusteredKey == e2.ClusteredKey && e.ClusteredKey2 == e2.ClusteredKey2).ShouldBeEquivalentTo(e2);

            repo.Received(1).TryGetById(e1.Id, out e1);
            repo.Received(1).FirstOrDefault(Arg.Any<Expression<Func<TestEntity, bool>>>());
        }

        [Fact]
        public void Query_Uses_Cache()
        {
            var e1 = _fixture.Create<TestEntity>();
            var e2 = _fixture.Create<TestEntity>();

            var indexCache = _fixture.Freeze<IIndexCache<TestEntity>>();
            var repo = _fixture.Freeze<IQueryableRepository<TestEntity>>();

            TestEntity dummy1;
            repo
                .TryGetById(e1.Id, out dummy1)
                .Returns(ci =>
                {
                    ci[1] = e1;
                    return true;
                });

            repo
                .Query(Arg.Any<Expression<Func<TestEntity, bool>>>())
                .ReturnsForAnyArgs(ci =>
                {
                    var q = ci.Arg<Expression<Func<TestEntity, bool>>>().Compile();
                    return q(e2) ? new[] { e2 } : null;
                });

            indexCache.Index(e1);

            var indexCacheResponsibility = _fixture.Create<IndexCacheResponsibility<TestEntity>>();

            indexCacheResponsibility.Query(e => e.ClusteredKey == e1.ClusteredKey && e.ClusteredKey2 == e1.ClusteredKey2).FirstOrDefault().ShouldBeEquivalentTo(e1);
            indexCacheResponsibility.Query(e => e.ClusteredKey == e2.ClusteredKey && e.ClusteredKey2 == e2.ClusteredKey2).FirstOrDefault().ShouldBeEquivalentTo(e2);

            repo.Received(1).TryGetById(e1.Id, out e1);
            repo.Received(1).Query(Arg.Any<Expression<Func<TestEntity, bool>>>());
        }
    }
}
