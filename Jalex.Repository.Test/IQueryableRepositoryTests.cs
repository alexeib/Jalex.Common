using System.Linq;
using Jalex.Logging.Loggers;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Should;

namespace Jalex.Repository.Test
{
    // ReSharper disable once InconsistentNaming
    public abstract class IQueryableRepositoryTests : ISimpleRepositoryTests
    {
        protected IQueryableRepository<TestEntity> _queryableRepository;

        protected IQueryableRepositoryTests(
            IQueryableRepository<TestEntity> sut, 
            IFixture fixture) : 
            base(sut, fixture)
        {
            _queryableRepository = sut;
        }

        [Fact]
        public void RetrievesEntitiesByQueryingForAttribute()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _queryableRepository.Query(r => _sampleTestEntitys.Select(e => e.Name).Contains(r.Name)).ToArray();

            retrievedTestEntitys.Length.ShouldBe(_sampleTestEntitys.Count());
            retrievedTestEntitys.Select(r => r.Name).Intersect(_sampleTestEntitys.Select(r => r.Name)).Count().ShouldBe(_sampleTestEntitys.Count());
        }

        [Fact]
        public void DoesNotRetrieveEntitiesByQueryingForNonExistantAttribute()
        {
            var fakeName = _fixture.Create<string>();

            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _queryableRepository.Query(r => r.Name == fakeName).ToArray();
            retrievedTestEntitys.ShouldBeEmpty();
        }
    }
}
