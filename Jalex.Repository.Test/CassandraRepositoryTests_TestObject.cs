using System;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests_TestObject : IQueryableRepositoryWithTtlTests<TestObject>
    {
        public CassandraRepositoryTests_TestObject()
            : base(createFixture())
        {
            
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Customize<CassandraRepository<TestObject>>(c => c.OmitAutoProperties());
            
            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepositoryWithTtl<TestObject>>(() =>
                                                               {
                                                                   var repo = fixture.Create<CassandraRepository<TestObject>>();
                                                                   return repo;
                                                               });
            fixture.Register<IQueryableRepository<TestObject>>(fixture.Create<IQueryableRepositoryWithTtl<TestObject>>);
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepositoryWithTtl<TestObject>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<Guid>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
