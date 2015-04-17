using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Logging.Loggers;
using Jalex.Repository.Test.Objects;
using Ploeh.AutoFixture;
using Xunit;

namespace Jalex.Repository.Test
{
    // ReSharper disable once InconsistentNaming
    public abstract class ISimpleRepositoryTests<T> : IDisposable
        where T : class, IObjectWithIdAndName, new()
    {
        protected readonly IFixture _fixture;
        private readonly ISimpleRepository<T> _testEntityRepository;
        protected readonly IEnumerable<T> _sampleTestEntitys;
        private readonly MemoryLogger _logger;

        protected ISimpleRepositoryTests(
            IFixture fixture)
        {
            _fixture = fixture;

            _logger = new MemoryLogger();

            _fixture.Inject<ILogger>(_logger);
            _testEntityRepository = _fixture.Create<ISimpleRepository<T>>();

            _sampleTestEntitys = _fixture.CreateMany<T>();
        }

        public virtual void Dispose()
        {
            _testEntityRepository.DeleteMany(_sampleTestEntitys.Select(r => r.Id));
            _logger.Clear();
        }

        [Fact]
        public void CreatesEntity()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Upsert);

            createResult.Success.Should().BeTrue();
            createResult.Value.Should()
                        .NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            T retrieved;
            var success = _testEntityRepository.TryGetById(sampleEntity.Id, out retrieved);

            success.Should().Be(true);
            retrieved.ShouldBeEquivalentTo(sampleEntity);
        }

        [Fact]
        public void CreatesManyEntities()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Insert).ToArray();

            createResult.All(r => r.Success).Should().BeTrue();
            createResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            createResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved;
                var success = _testEntityRepository.TryGetById(entity.Id, out retrieved);

                success.Should().Be(true);
                retrieved.ShouldBeEquivalentTo(entity);
            }
        }


        [Fact]
        public void DoesNotCreateExistingEntities()
        {
            var sampleEntity = _sampleTestEntitys.First();

            OperationResult<Guid> createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Insert);
            createResult.Success.Should().BeTrue();

            var createResultExisting = _testEntityRepository.Save(sampleEntity, WriteMode.Insert);

            createResultExisting.Success.Should().BeFalse();
            createResultExisting.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public void DoesNotCreateEntitiesWithDuplicateIds()
        {
            Guid id = _fixture.Create<Guid>();
            var obj1 = _fixture.Create<T>();
            var obj2 = _fixture.Create<T>();

            obj1.Id = id;
            obj2.Id = id;

            if (typeof (T) == typeof (TestObjectWithClustering))
            {
                var results = _testEntityRepository.SaveMany(new[]
                                               {
                                                   obj1,
                                                   obj2
                                               },
                                               WriteMode.Insert);
                results.Should()
                       .OnlyContain(r => r.Success);

                _testEntityRepository.Delete(id);
            }
            else
            {
                var exception = Assert.Throws<DuplicateIdException>(() => _testEntityRepository.SaveMany(new[]
                                                                                                         {
                                                                                                             obj1,
                                                                                                             obj2
                                                                                                         },
                                                                                                         WriteMode.Insert));
                exception.Should()
                         .NotBeNull();
            }
        }

        [Fact]
        public void DeletesExistingTestEntities()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Upsert);
            createResult.Success.Should().BeTrue();

            var deleteResult = _testEntityRepository.Delete(sampleEntity.Id);

            deleteResult.Success.Should().BeTrue();
            deleteResult.Messages.Should().BeEmpty();

            T retrieved;
            var success = _testEntityRepository.TryGetById(sampleEntity.Id, out retrieved);
            success.Should().BeFalse();
            retrieved.Should().BeNull();
        }

        [Fact]
        public void FailsToDeleteNonExistingEntities()
        {
            var fakeEntityId = _fixture.Create<Guid>();
            var deleteResult = _testEntityRepository.Delete(fakeEntityId);
            deleteResult.Success.Should().BeFalse();
        }

        [Fact]
        public void DeletesEntitiesUsingQuery()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Upsert);
            createResult.Success.Should().BeTrue();

            var deleteResult = _testEntityRepository.DeleteWhere(e => e.Id == sampleEntity.Id);

            deleteResult.Success.Should().BeTrue();
            deleteResult.Messages.Should().BeEmpty();

            T retrieved;
            var success = _testEntityRepository.TryGetById(sampleEntity.Id, out retrieved);
            success.Should().BeFalse();
            retrieved.Should().BeNull();
        }

        [Fact]
        public void RetrievesOneEntityById()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            var targetEntity = _sampleTestEntitys.First();

            T retrieved;
            var success = _testEntityRepository.TryGetById(targetEntity.Id, out retrieved);

            success.Should().BeTrue();
            retrieved.ShouldBeEquivalentTo(targetEntity);
        }

        [Fact]
        public void RetrievesAllEntities()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            var retrievedTestEntitys = _testEntityRepository.GetAll().ToArray();

            retrievedTestEntitys.Should().HaveCount(_sampleTestEntitys.Count());
            retrievedTestEntitys.ShouldAllBeEquivalentTo(_sampleTestEntitys);
        }

        [Fact]
        public void DoesNotRetrieveNonExistantEntitiesById()
        {
            var fakeEntityIds = _fixture.Create<Guid>();

            T retrieved;
            var success = _testEntityRepository.TryGetById(fakeEntityIds, out retrieved);
            success.Should().BeFalse();
            retrieved.Should().BeNull();
        }

        [Fact]
        public void UpdatesExistingEntity()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            var testEntityToUpdate = _sampleTestEntitys.Last();
            testEntityToUpdate.Name = "changed name";

            var updateResult = _testEntityRepository.Save(testEntityToUpdate, WriteMode.Update);

            updateResult.Success.Should().BeTrue();
            updateResult.Messages.Should().BeEmpty();

            T retrievedTestEntity;
             var success = _testEntityRepository.TryGetById(testEntityToUpdate.Id, out retrievedTestEntity);

            success.Should().BeTrue();
            retrievedTestEntity.Should().NotBeNull();

            retrievedTestEntity.Name.Should().Be(testEntityToUpdate.Name);
        }

        [Fact]
        public void UpdatesExistingEntityWithUpsert()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert);
            createResult.All(r => r.Success).Should().BeTrue();

            var testEntityToUpdate = _sampleTestEntitys.Last();
            testEntityToUpdate.Name = "changed name";

            var updateResult = _testEntityRepository.Save(testEntityToUpdate, WriteMode.Upsert);

            updateResult.Success.Should().BeTrue();
            updateResult.Messages.Should().BeEmpty();

            T retrievedTestEntity;
            var success = _testEntityRepository.TryGetById(testEntityToUpdate.Id, out retrievedTestEntity);

            success.Should().BeTrue();
            retrievedTestEntity.Should().NotBeNull();

            retrievedTestEntity.Name.Should().Be(testEntityToUpdate.Name);
        }

        [Fact]
        public void CreatesManyEntitiesWithUpsert()
        {
            var createResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert).ToArray();

            createResult.All(r => r.Success).Should().BeTrue();
            createResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            createResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved;
                var success = _testEntityRepository.TryGetById(entity.Id, out retrieved);

                success.Should().Be(true);
                retrieved.ShouldBeEquivalentTo(entity);
            }
        }

        [Fact]
        public void CreatesNonExistingEntitiesAndUpdatesExistingWithUpsert()
        {
            var existingEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.Save(existingEntity, WriteMode.Insert);
            createResult.Success.Should().BeTrue();

            existingEntity.Name = _fixture.Create<string>();

            var upsertResult = _testEntityRepository.SaveMany(_sampleTestEntitys, WriteMode.Upsert).ToArray();

            upsertResult.All(r => r.Success).Should().BeTrue();
            upsertResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            upsertResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved;
                var success = _testEntityRepository.TryGetById(entity.Id, out retrieved);

                success.Should().Be(true);
                retrieved.ShouldBeEquivalentTo(entity);
            }
        }

        [Fact]
        public void InsertsNewEntityWithUpsert()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Upsert);

            createResult.Success.Should().BeTrue();
            createResult.Value.Should().NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            T retrieved;
            var success = _testEntityRepository.TryGetById(sampleEntity.Id, out retrieved);

            success.Should().Be(true);
            retrieved.ShouldBeEquivalentTo(sampleEntity);
        }

        [Fact]
        public void FailsToUpdateNonExistingEntity()
        {
            var nonexistentEntity = _fixture.Create<T>();

            var updateResult = _testEntityRepository.Save(nonexistentEntity, WriteMode.Update);

            updateResult.Success.Should().BeFalse();
            updateResult.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public void FailsToUpdateEntityWithNullId()
        {
            var invalidIdTestEntity = new T { Id = Guid.Empty };
            _testEntityRepository.Invoking(r => r.Save(invalidIdTestEntity, WriteMode.Update)).ShouldThrow<InvalidOperationException>();
        }
    }
}
