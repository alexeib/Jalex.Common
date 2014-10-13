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
            createResult.Value.Should().NotBeNullOrEmpty();
            sampleEntity.Id.Should().NotBeNullOrEmpty();
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
            createResult.All(r => !string.IsNullOrEmpty(r.Value)).Should().BeTrue();
            _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).Should().BeTrue();
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

            OperationResult<string> createResult = _testEntityRepository.Save(sampleEntity, WriteMode.Insert);
            createResult.Success.Should().BeTrue();

            var createResultExisting = _testEntityRepository.Save(sampleEntity, WriteMode.Insert);

            createResultExisting.Success.Should().BeFalse();
            createResultExisting.Messages.Should().NotBeEmpty();
            _logger.Logs.Should().NotBeEmpty();
        }

        [Fact]
        public void DoesNotCreateEntitiesWithInvalidIds()
        {
            var exception = Assert.Throws<IdFormatException>(() => _testEntityRepository.Save(new T { Id = "FakeId", Name = "FakeName"}, WriteMode.Insert));
            exception.Should().NotBeNull();
        }

        [Fact]
        public void DoesNotCreateEntitiesWithDuplicateIds()
        {
            string id = _fixture.Create<string>();

            var exception = Assert.Throws<DuplicateIdException>(() => _testEntityRepository.SaveMany(new[]
                                                                                                {
                                                                                                    new T { Id = id, Name = "SameId" },
                                                                                                    new T { Id = id, Name = "SameId" }
                                                                                                }, WriteMode.Insert));
            exception.Should().NotBeNull();
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
            var fakeEntityId = _fixture.Create<string>();
            var deleteResult = _testEntityRepository.Delete(fakeEntityId);
            deleteResult.Success.Should().BeFalse();
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
            var fakeEntityIds = _fixture.Create<string>();

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
            createResult.All(r => !string.IsNullOrEmpty(r.Value)).Should().BeTrue();
            _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).Should().BeTrue();
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
            upsertResult.All(r => !string.IsNullOrEmpty(r.Value)).Should().BeTrue();
            _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).Should().BeTrue();
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
            createResult.Value.Should().NotBeNullOrEmpty();
            sampleEntity.Id.Should().NotBeNullOrEmpty();
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
            var invalidIdTestEntity = new T { Id = null };
            _testEntityRepository.Invoking(r => r.Save(invalidIdTestEntity, WriteMode.Update)).ShouldThrow<InvalidOperationException>();
        }
    }
}
