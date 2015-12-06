using System;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests_TestObjectWithClustering : IQueryableRepositoryWithTtlTests<TestObjectWithClustering>
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
            fixture.Register<IQueryableRepositoryWithTtl<TestObjectWithClustering>>(() =>
                                                               {
                                                                   var repo = fixture.Create<CassandraRepository<TestObjectWithClustering>>();
                                                                   return repo;
                                                               });
            fixture.Register<IQueryableRepository<TestObjectWithClustering>>(fixture.Create<IQueryableRepositoryWithTtl<TestObjectWithClustering>>);
            fixture.Register<ISimpleRepository<TestObjectWithClustering>>(fixture.Create<IQueryableRepositoryWithTtl<TestObjectWithClustering>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<Guid>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
