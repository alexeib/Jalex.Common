using System.Linq;
using FluentAssertions;
using Jalex.Caching.Memory;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Services.Caching;
using Jalex.Services.Test.Fixtures;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Services.Test.Caching
{
    public class IndexCacheFactoryTests
    {
        private readonly IFixture _fixture;

        public IndexCacheFactoryTests()
        {
            _fixture = new Fixture();

            registerCache();

            _fixture.Register<IIndexCacheFactory>(_fixture.Create<IndexCacheFactory>);
            _fixture.Register<IReflectedTypeDescriptorProvider>(_fixture.Create<ReflectedTypeDescriptorProvider>);
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
        public void Creates_Index_Cache_For_Object_With_Clustered_Keys()
        {
            var sut = _fixture.Create<IIndexCacheFactory>();
            var indexCaches = sut.CreateIndexCachesForType<TestEntity>();

            indexCaches
                .Should()
                .ContainSingle(c => c.IndexedProperties.SequenceEqual(new[] { "ClusteredKey", "ClusteredKey2" }));
        }
    }
}
