using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Utils;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests : IQueryableRepositoryTests
    {
        public CassandraRepositoryTests()
            : base(createFixture())
        {
            
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepository<TestObject>>(fixture.Create<CassandraRepository<TestObject>>);
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
