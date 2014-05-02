using EmitMapper;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Memory;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MappingSimpleRepositoryTests : ISimpleRepositoryTests
    {
        private static readonly GuidIdProvider _idProvider = new GuidIdProvider();

        public MappingSimpleRepositoryTests()
            : base(createRepository(), createFixture())
        {
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();            

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(_idProvider.GenerateNewId);

            return fixture;
        }

        private static MappingSimpleRepository<TestObject, TestEntity> createRepository()
        {
            ISimpleRepository<TestEntity> entityRepository = new MemoryRepository<TestEntity>(_idProvider);

            var repository = new MappingSimpleRepository<TestObject, TestEntity>(
                entityRepository,
                ObjectMapperManager.DefaultInstance.GetMapper<TestObject, TestEntity>(),
                ObjectMapperManager.DefaultInstance.GetMapper<TestEntity, TestObject>());

            return repository;
        }
    }
}
