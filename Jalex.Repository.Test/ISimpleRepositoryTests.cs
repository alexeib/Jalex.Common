using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Logging.Loggers;
using Machine.Specifications.Utility;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Should;

namespace Jalex.Repository.Test
{
    // ReSharper disable once InconsistentNaming
    public abstract class ISimpleRepositoryTests : IDisposable
    {
        protected IFixture _fixture;
        protected ISimpleRepository<TestObject> _testEntityRepository;
        protected IEnumerable<TestObject> _sampleTestEntitys;
        protected MemoryLogger _logger;

        protected ISimpleRepositoryTests(
            ISimpleRepository<TestObject> sut,
            IFixture fixture)
        {
            _fixture = fixture;

            _logger = new MemoryLogger();

            _testEntityRepository = sut;
            _testEntityRepository.Logger = _logger;

            _sampleTestEntitys = _fixture.CreateMany<TestObject>();
        }

        public virtual void Dispose()
        {
            _testEntityRepository.Delete(_sampleTestEntitys.Select(r => r.Id));
            _logger.Clear();
        }


        [Fact]
        public void CreatesEntities()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys).ToArray();

            createResult.All(r => r.Success).ShouldBeTrue();
            createResult.All(r => !string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
            _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).ShouldBeTrue();
            createResult.All(r => !r.Messages.Any()).ShouldBeTrue();
            _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldNotBeEmpty();
        }


        [Fact]
        public void DoesNotCreateExistingEntities()
        {
            IEnumerable<OperationResult<string>> createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var createResultExisting = _testEntityRepository.Create(_sampleTestEntitys).ToArray();

            createResultExisting.All(r => !r.Success).ShouldBeTrue();
            createResultExisting.All(r => string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
            createResultExisting.All(r => r.Messages.Any()).ShouldBeTrue();
            _logger.Logs.ShouldNotBeEmpty();
        }

        [Fact]
        public void DoesNotCreateEntitiesWithInvalidIds()
        {
            var exception = Assert.Throws<IdFormatException>(() => _testEntityRepository.Create(new[] { new TestObject { Id = "FakeId", Name = "FakeName" } }));
            exception.ShouldNotBeNull();
        }

        [Fact]
        public void DoesNotCreateEntitiesWithDuplicateIds()
        {
            string id = _fixture.Create<string>();

            var exception = Assert.Throws<DuplicateIdException>(() => _testEntityRepository.Create(new[]
                                                                                                {
                                                                                                    new TestObject { Id = id, Name = "SameId" },
                                                                                                    new TestObject { Id = id, Name = "SameId" }
                                                                                                }));
            exception.ShouldNotBeNull();
        }

        [Fact]
        public void DeletesExistingTestEntities()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var deleteResult = _testEntityRepository.Delete(_sampleTestEntitys.Select(r => r.Id)).ToArray();

            deleteResult.All(r => r.Success).ShouldBeTrue();
            deleteResult.All(r => !r.Messages.Any()).ShouldBeTrue();
            _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldBeEmpty();
        }

        [Fact]
        public void FailsToDeleteNonExistingEntities()
        {
            var fakeEntityIds = _fixture.CreateMany<string>().ToArray();

            var deleteResult = _testEntityRepository.Delete(fakeEntityIds).ToArray();

            deleteResult.Length.ShouldBe(fakeEntityIds.Length);
            deleteResult.All(r => !r.Success).ShouldBeTrue();
        }

        [Fact]
        public void RetrievesOneEntityById()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var targetTestEntityId = _sampleTestEntitys.First().Id;

            var retrievedTestEntitys = _testEntityRepository.GetById(targetTestEntityId);

            retrievedTestEntitys.ShouldNotBeNull();
            retrievedTestEntitys.Id.ShouldBe(targetTestEntityId);
        }

        [Fact]
        public void RetrievesSeveralEntitiesById()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ToArray();

            retrievedTestEntitys.Length.ShouldBe(_sampleTestEntitys.Count());
            retrievedTestEntitys.Select(r => r.Id).Intersect(_sampleTestEntitys.Select(r => r.Id)).Count().ShouldBe(_sampleTestEntitys.Count());
        }

        [Fact] 
        public void RetrievesAllEntities()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _testEntityRepository.GetAll().ToArray();

            retrievedTestEntitys.Length.ShouldBeGreaterThanOrEqualTo(_sampleTestEntitys.Count());
            retrievedTestEntitys.Select(r => r.Id).Intersect(_sampleTestEntitys.Select(r => r.Id)).Count().ShouldBe(_sampleTestEntitys.Count());
        }

        [Fact]
        public void DoesNotRetrieveNonExistantEntitiesById()
        {
            var fakeEntityIds = _fixture.CreateMany<string>().ToArray();
            var retrievedTestEntitys = _testEntityRepository.GetByIds(fakeEntityIds);
            retrievedTestEntitys.ShouldBeEmpty();
        }

        [Fact]
        public void UpdatesExistingEntity()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var testEntityToUpdate = _sampleTestEntitys.Last();
            testEntityToUpdate.Name = "changed name";

            var updateResult = _testEntityRepository.Update(testEntityToUpdate);

            updateResult.Success.ShouldBeTrue();
            updateResult.Messages.ShouldBeEmpty();

            var retrievedTestEntity = _testEntityRepository.GetById(testEntityToUpdate.Id);
            retrievedTestEntity.ShouldNotBeNull();

            retrievedTestEntity.Name.ShouldBe(testEntityToUpdate.Name);
        }

        [Fact]
        public void FailsToUpdateNonExistingEntity()
        {
            var nonexistentEntity = _fixture.Create<TestObject>();

            var updateResult = _testEntityRepository.Update(nonexistentEntity);

            updateResult.Success.ShouldBeFalse();
            updateResult.Messages.ShouldNotBeEmpty();
        }

        [Fact]
        public void FailsToUpdateEntityWithNullId()
        {
            var invalidIdTestEntity = new TestObject { Id = null };

            Assert.Throws<ArgumentNullException>(() => _testEntityRepository.Update(invalidIdTestEntity));
        }
    }
}
