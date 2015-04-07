using System;
using EmitMapper;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Repository.Test;
using Jalex.Repository.Test.Objects;
using Jalex.Services.Repository;
using Ploeh.AutoFixture;

namespace Jalex.Services.Test.Repository
{
    public class MappingResponsibilityTests : IQueryableRepositoryTests<TestObject>
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
            fixture.Register<IQueryableRepository<TestObject>>(() =>
                                                            {
                                                                var entityRepository = fixture.Create<MemoryRepository<TestEntity>>();
                                                                return new MappingResponsibility<TestObject, TestEntity>(
                                                                    entityRepository,
                                                                    ObjectMapperManager.DefaultInstance.GetMapper<TestObject, TestEntity>(),
                                                                    ObjectMapperManager.DefaultInstance.GetMapper<TestEntity, TestObject>(),
                                                                    fixture.Create<IReflectedTypeDescriptorProvider>());
                                                            });
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<Guid>(() => fixture.Create<IIdProvider>().GenerateNewId());

            return fixture;
        }
    }
}
