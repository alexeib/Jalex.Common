using System;
using System.Linq;
using FluentAssertions;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Repository.Test
{
    // ReSharper disable once InconsistentNaming
    public abstract class IQueryableRepositoryTests<T> : ISimpleRepositoryTests<T>
        where T : class, IObjectWithIdAndName, new()
    {
        private readonly IQueryableRepository<T> _queryableRepository;

        protected IQueryableRepositoryTests(
            IFixture fixture) :
            base(fixture)
        {
            _queryableRepository = _fixture.Create<IQueryableRepository<T>>();            
        }

        [Fact]
        public void RetrievesEntitiesByQueryingForAttribute()
        {
            var createResult = _queryableRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.QueryAsync(r => r.Name == nameToFind).Result.ToArray();

            retrievedTestEntitys.Length.Should().Be(1);
            retrievedTestEntitys.First().Name.Should().Be(nameToFind);
        }

        [Fact]
        public void Retrieves_Projection_To_Same_Property()
        {
            var createResult = _queryableRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.ProjectAsync(r => r.Name, r => r.Name == nameToFind).Result.ToArray();

            retrievedTestEntitys.Length.Should().Be(1);
            retrievedTestEntitys.First().Should().Be(nameToFind);
        }

        [Fact]
        public void Retrieves_Projection_To_Different_Property()
        {
            var createResult = _queryableRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var entity = _sampleTestEntitys.First();
            var idToFind = entity.Id;
            string nameToFind = entity.Name;
            var retrievedTestEntitys = _queryableRepository.ProjectAsync(r => r.Name, r => r.Id == idToFind).Result.ToArray();

            retrievedTestEntitys.Length.Should().Be(1);
            retrievedTestEntitys.First().Should().Be(nameToFind);
        }

        [Fact]
        public void DoesNotRetrieveEntitiesByQueryingForNonExistantAttribute()
        {
            var fakeName = _fixture.Create<string>();

            var createResult = _queryableRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var retrievedTestEntitys = _queryableRepository.QueryAsync(r => r.Name == fakeName).Result.ToArray();
            retrievedTestEntitys.Should().BeEmpty();
        }

        [Fact]
        public void Retrieves_First_Entity_Queried_By_Attribute()
        {
            var createResult = _queryableRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.FirstOrDefaultAsync(r => r.Name == nameToFind).Result;

            retrievedTestEntitys
                .ShouldBeEquivalentTo(_sampleTestEntitys.First(),
                                      opts => opts
                                                  .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000))
                                                  .WhenTypeIs<DateTime>());
        }

        [Fact]
        public void Returns_Default_Value_When_Entity_Not_Found()
        {
            string nameToFind = _fixture.Create<string>();
            var retrievedTestEntity = _queryableRepository.FirstOrDefaultAsync(r => r.Name == nameToFind).Result;

            retrievedTestEntity.Should().BeNull();
        }

        [Fact]
        public void DeletesEntitiesUsingQuery()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _queryableRepository.SaveAsync(sampleEntity, WriteMode.Upsert).Result;
            createResult.Success.Should().BeTrue();

            var deleteResult = _queryableRepository.DeleteWhereAsync(e => e.Id == sampleEntity.Id).Result;

            deleteResult.Success.Should().BeTrue();
            deleteResult.Messages.Should().BeEmpty();

            T retrieved = _queryableRepository.GetByIdAsync(sampleEntity.Id).Result;
            retrieved.Should().BeNull();
        }
    }
}
