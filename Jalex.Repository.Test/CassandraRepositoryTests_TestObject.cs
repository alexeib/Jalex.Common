using System;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Cassandra;
using Jalex.Repository.IdProviders;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests_TestObject : IQueryableRepositoryTests<TestObject>
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
            fixture.Register<IQueryableRepository<TestObject>>(() =>
                                                               {
                                                                   var repo = fixture.Create<CassandraRepository<TestObject>>();
                                                                   return repo;
                                                               });
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<Guid>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
