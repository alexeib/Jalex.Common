using System.Linq;
using Jalex.Infrastructure.Repository;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Should;

namespace Jalex.Repository.Test
{
    // ReSharper disable once InconsistentNaming
    public abstract class IQueryableRepositoryTests : ISimpleRepositoryTests
    {
        protected IQueryableRepository<TestObject> _queryableRepository;

        protected IQueryableRepositoryTests(
            IFixture fixture) : 
            base(fixture)
        {
            _queryableRepository = _fixture.Create<IQueryableRepository<TestObject>>();            
        }

        [Fact]
        public void RetrievesEntitiesByQueryingForAttribute()
        {
            var createResult = _queryableRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.Query(r => r.Name == nameToFind).ToArray();

            retrievedTestEntitys.Length.ShouldBe(1);
            retrievedTestEntitys.First().Name.ShouldBe(nameToFind);
        }

        [Fact]
        public void DoesNotRetrieveEntitiesByQueryingForNonExistantAttribute()
        {
            var fakeName = _fixture.Create<string>();

            var createResult = _queryableRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _queryableRepository.Query(r => r.Name == fakeName).ToArray();
            retrievedTestEntitys.ShouldBeEmpty();
        }
    }
}
