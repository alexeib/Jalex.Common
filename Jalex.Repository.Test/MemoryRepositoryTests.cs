using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MemoryRepositoryTests : IQueryableRepositoryTests
    {
        public MemoryRepositoryTests()
            : base(createRepository(), createFixture())
        {
            
        }

        private static MemoryRepository<TestObject> createRepository()
        {
            var cassandraIdGenerator = new GuidIdProvider();
            return new MemoryRepository<TestObject>(cassandraIdGenerator);
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
