using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests_TestObjectWithClustering : IQueryableRepositoryTests<TestObjectWithClustering>
    {
        public CassandraRepositoryTests_TestObjectWithClustering()
            : base(createFixture())
        {
            
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Customize<CassandraRepository<TestObjectWithClustering>>(c => c.OmitAutoProperties());
            
            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepository<TestObjectWithClustering>>(() =>
                                                               {
                                                                   var repo = fixture.Create<CassandraRepository<TestObjectWithClustering>>();
                                                                   repo.Logger = fixture.Create<ILogger>();
                                                                   return repo;
                                                               });
            fixture.Register<ISimpleRepository<TestObjectWithClustering>>(fixture.Create<IQueryableRepository<TestObjectWithClustering>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
