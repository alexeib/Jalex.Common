using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Objects;
using Jalex.Logging.Loggers;
using Jalex.Repository.Extensions;
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
        protected ISimpleRepository<TestEntity> _testEntityRepository;
        protected IEnumerable<TestEntity> _sampleTestEntitys;
        protected MemoryLogger _logger;

        protected ISimpleRepositoryTests(
            ISimpleRepository<TestEntity> sut,
            IFixture fixture)
        {
            _fixture = fixture;

            _logger = new MemoryLogger();

            _testEntityRepository = sut;
            _testEntityRepository.Logger = _logger;

            _sampleTestEntitys = _fixture.CreateMany<TestEntity>();
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
            var exception = Assert.Throws<FormatException>(() => _testEntityRepository.Create(new[] { new TestEntity { Id = "FakeId", Name = "FakeName" } }));
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
            retrievedTestEntitys.IgnoredProperty.ShouldBeNull();
        }

        [Fact]
        public void RetrievesSeveralEntitiesById()
        {
            var createResult = _testEntityRepository.Create(_sampleTestEntitys);
            createResult.All(r => r.Success).ShouldBeTrue();

            var retrievedTestEntitys = _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ToArray();

            retrievedTestEntitys.Length.ShouldBe(_sampleTestEntitys.Count());
            retrievedTestEntitys.Select(r => r.Id).Intersect(_sampleTestEntitys.Select(r => r.Id)).Count().ShouldBe(_sampleTestEntitys.Count());
            retrievedTestEntitys.Each(e => e.IgnoredProperty.ShouldBeNull());
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
            testEntityToUpdate.IgnoredProperty = "changed ignore value";

            var updateResult = _testEntityRepository.Update(testEntityToUpdate);

            updateResult.Success.ShouldBeTrue();
            updateResult.Messages.ShouldBeEmpty();

            var retrievedTestEntity = _testEntityRepository.GetById(testEntityToUpdate.Id);
            retrievedTestEntity.ShouldNotBeNull();

            retrievedTestEntity.Name.ShouldBe(testEntityToUpdate.Name);
            retrievedTestEntity.IgnoredProperty.ShouldBeNull();
        }

        [Fact]
        public void FailsToUpdateNonExistingEntity()
        {
            var nonexistentEntity = _fixture.Create<TestEntity>();

            var updateResult = _testEntityRepository.Update(nonexistentEntity);

            updateResult.Success.ShouldBeFalse();
            updateResult.Messages.ShouldNotBeEmpty();
        }

        [Fact]
        public void FailsToUpdateEntityWithNullId()
        {
            var invalidIdTestEntity = new TestEntity { Id = null };

            Assert.Throws<ArgumentNullException>(() => _testEntityRepository.Update(invalidIdTestEntity));
        }
    }
}
