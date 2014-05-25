using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Repository.Utils;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MemoryRepositoryTests : IQueryableRepositoryTests
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
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
