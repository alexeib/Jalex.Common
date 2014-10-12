using Jalex.Caching.Memory;
using Jalex.Caching.Test.Fixtures;
using Jalex.Infrastructure.Caching;
using Ploeh.AutoFixture;

namespace Jalex.Caching.Test.Memory
{
    public class MemoryCacheTests : CacheTests
    {
        public MemoryCacheTests() 
            : base(createFixture())
        {
        }

        private static IFixture createFixture()
        {
            var fixture = new Fixture();
            fixture.Register<ICache<string, TestEntity>>(fixture.Create<MemoryCache<string, TestEntity>>);
            return fixture;
        }
    }
}