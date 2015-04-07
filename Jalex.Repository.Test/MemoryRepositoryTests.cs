using System;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MemoryRepositoryTests : IQueryableRepositoryTests<TestObject>
    {
        public MemoryRepositoryTests()
            : base(createFixture())
        {

        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepository<TestObject>>(fixture.Create<MemoryRepository<TestObject>>);
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            GuidIdProvider idProvider = new GuidIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<Guid>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
