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
            var createResult = _queryableRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.Query(r => r.Name == nameToFind).ToArray();

            retrievedTestEntitys.Length.Should().Be(1);
            retrievedTestEntitys.First().Name.Should().Be(nameToFind);
        }

        [Fact]
        public void DoesNotRetrieveEntitiesByQueryingForNonExistantAttribute()
        {
            var fakeName = _fixture.Create<string>();

            var createResult = _queryableRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            var retrievedTestEntitys = _queryableRepository.Query(r => r.Name == fakeName).ToArray();
            retrievedTestEntitys.Should().BeEmpty();
        }

        [Fact]
        public void Retrieves_First_Entity_Queried_By_Attribute()
        {
            var createResult = _queryableRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            string nameToFind = _sampleTestEntitys.First().Name;
            var retrievedTestEntitys = _queryableRepository.FirstOrDefault(r => r.Name == nameToFind);

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
            var retrievedTestEntity = _queryableRepository.FirstOrDefault(r => r.Name == nameToFind);

            retrievedTestEntity.Should().BeNull();
        }

        [Fact]
        public void DeletesEntitiesUsingQuery()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _queryableRepository.Save(sampleEntity, WriteMode.Upsert);
            createResult.Success.Should().BeTrue();

            var deleteResult = _queryableRepository.DeleteWhere(e => e.Id == sampleEntity.Id);

            deleteResult.Success.Should().BeTrue();
            deleteResult.Messages.Should().BeEmpty();

            T retrieved;
            var success = _queryableRepository.TryGetById(sampleEntity.Id, out retrieved);
            success.Should().BeFalse();
            retrieved.Should().BeNull();
        }
    }
}
