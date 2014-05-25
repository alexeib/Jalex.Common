using EmitMapper;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Repository.Test.Objects;
using Jalex.Repository.Utils;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MappingResponsibilityTests : ISimpleRepositoryTests
    {
        public MappingResponsibilityTests()
            : base(createFixture())
        {
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Register<IIdProvider>(fixture.Create<GuidIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<ISimpleRepository<TestObject>>(() =>
                                                            {
                                                                var entityRepository = fixture.Create<MemoryRepository<TestEntity>>();
                                                                return new MappingResponsibility<TestObject, TestEntity>(
                                                                    entityRepository,
                                                                    ObjectMapperManager.DefaultInstance.GetMapper<TestObject, TestEntity>(),
                                                                    ObjectMapperManager.DefaultInstance.GetMapper<TestEntity, TestObject>());
                                                            });

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(() => fixture.Create<IIdProvider>().GenerateNewId());

            return fixture;
        }
    }
}
