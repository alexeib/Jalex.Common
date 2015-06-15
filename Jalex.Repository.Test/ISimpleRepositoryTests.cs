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

            _sampleTestEntitys = _fixture.CreateMany<T>().ToList();
        }

        public virtual void Dispose()
        {
            _testEntityRepository.DeleteManyAsync(_sampleTestEntitys.Select(r => r.Id)).Wait();
            _logger.Clear();
        }

        [Fact]
        public virtual void CreatesEntity()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.SaveAsync(sampleEntity, WriteMode.Upsert).Result;

            createResult.Success.Should().BeTrue();
            createResult.Value.Should()
                        .NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            T retrieved = _testEntityRepository.GetByIdAsync(sampleEntity.Id).Result;

            retrieved.ShouldBeEquivalentTo(sampleEntity);
        }

        [Fact]
        public virtual void CreatesManyEntities()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Insert).Result.ToArray();

            createResult.All(r => r.Success).Should().BeTrue();
            createResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            createResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved = _testEntityRepository.GetByIdAsync(entity.Id).Result;

                retrieved.ShouldBeEquivalentTo(entity);
            }
        }


        [Fact]
        public virtual void DoesNotCreateExistingEntities()
        {
            var sampleEntity = _sampleTestEntitys.First();

            OperationResult<Guid> createResult = _testEntityRepository.SaveAsync(sampleEntity, WriteMode.Insert).Result;
            createResult.Success.Should().BeTrue();

            var createResultExisting = _testEntityRepository.SaveAsync(sampleEntity, WriteMode.Insert).Result;

            createResultExisting.Success.Should().BeFalse();
            createResultExisting.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public virtual void DoesNotCreateEntitiesWithDuplicateIds()
        {
            Guid id = _fixture.Create<Guid>();
            var obj1 = _fixture.Create<T>();
            var obj2 = _fixture.Create<T>();

            obj1.Id = id;
            obj2.Id = id;

            if (typeof (T) == typeof (TestObjectWithClustering))
            {
                var results = _testEntityRepository.SaveManyAsync(new[]
                                                                  {
                                                                      obj1,
                                                                      obj2
                                                                  },
                                                                  WriteMode.Insert)
                                                   .Result;
                results.Should()
                       .OnlyContain(r => r.Success);

                _testEntityRepository.DeleteAsync(id).Wait();
            }
            else
            {
                _testEntityRepository.Invoking(r =>
                                               {
                                                   r.SaveManyAsync(new[]
                                                                   {
                                                                       obj1,
                                                                       obj2
                                                                   },
                                                                   WriteMode.Insert)
                                                    .Wait();
                                               })
                                     .ShouldThrow<AggregateException>()
                                     .And.InnerExceptions.Should()
                                     .OnlyContain(e => e is DuplicateIdException || e is AggregateException);
            }
        }

        [Fact]
        public virtual void DeletesExistingTestEntities()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.SaveAsync(sampleEntity, WriteMode.Upsert).Result;
            createResult.Success.Should().BeTrue();

            var deleteResult = _testEntityRepository.DeleteAsync(sampleEntity.Id).Result;

            deleteResult.Success.Should().BeTrue();
            deleteResult.Messages.Should().BeEmpty();

            T retrieved = _testEntityRepository.GetByIdAsync(sampleEntity.Id).Result;
            retrieved.Should().BeNull();
        }

        [Fact]
        public virtual void FailsToDeleteNonExistingEntities()
        {
            var fakeEntityId = _fixture.Create<Guid>();
            var deleteResult = _testEntityRepository.DeleteAsync(fakeEntityId).Result;
            deleteResult.Success.Should().BeFalse();
        }

        [Fact]
        public void RetrievesOneEntityById()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var targetEntity = _sampleTestEntitys.First();

            T retrieved = _testEntityRepository.GetByIdAsync(targetEntity.Id).Result;

            retrieved.ShouldBeEquivalentTo(targetEntity);
        }

        [Fact]
        public void RetrievesAllEntities()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert)
                                                    .Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var retrievedTestEntitys = _testEntityRepository.GetAllAsync().Result.ToArray();

            retrievedTestEntitys.Should().HaveCount(_sampleTestEntitys.Count());
            retrievedTestEntitys.ShouldAllBeEquivalentTo(_sampleTestEntitys);
        }

        [Fact]
        public void DoesNotRetrieveNonExistantEntitiesById()
        {
            var fakeEntityIds = _fixture.Create<Guid>();

            T retrieved = _testEntityRepository.GetByIdAsync(fakeEntityIds).Result;
            retrieved.Should().BeNull();
        }

        [Fact]
        public virtual void UpdatesExistingEntity()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var testEntityToUpdate = _sampleTestEntitys.Last();
            testEntityToUpdate.Name = "changed name";

            var updateResult = _testEntityRepository.SaveAsync(testEntityToUpdate, WriteMode.Update).Result;

            updateResult.Success.Should().BeTrue();
            updateResult.Messages.Should().BeEmpty();

            T retrievedTestEntity = _testEntityRepository.GetByIdAsync(testEntityToUpdate.Id).Result;

            retrievedTestEntity.Should().NotBeNull();

            retrievedTestEntity.Name.Should().Be(testEntityToUpdate.Name);
        }

        [Fact]
        public virtual void UpdatesExistingEntityWithUpsert()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result;
            createResult.All(r => r.Success).Should().BeTrue();

            var testEntityToUpdate = _sampleTestEntitys.Last();
            testEntityToUpdate.Name = "changed name";

            var updateResult = _testEntityRepository.SaveAsync(testEntityToUpdate, WriteMode.Upsert).Result;

            updateResult.Success.Should().BeTrue();
            updateResult.Messages.Should().BeEmpty();

            T retrievedTestEntity = _testEntityRepository.GetByIdAsync(testEntityToUpdate.Id).Result;

            retrievedTestEntity.Should().NotBeNull();

            retrievedTestEntity.Name.Should().Be(testEntityToUpdate.Name);
        }

        [Fact]
        public virtual void CreatesManyEntitiesWithUpsert()
        {
            var createResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result.ToArray();

            createResult.All(r => r.Success).Should().BeTrue();
            createResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            createResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved = _testEntityRepository.GetByIdAsync(entity.Id).Result;

                retrieved.ShouldBeEquivalentTo(entity);
            }
        }

        [Fact]
        public virtual void CreatesNonExistingEntitiesAndUpdatesExistingWithUpsert()
        {
            var existingEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.SaveAsync(existingEntity, WriteMode.Insert).Result;
            createResult.Success.Should().BeTrue();

            existingEntity.Name = _fixture.Create<string>();

            var upsertResult = _testEntityRepository.SaveManyAsync(_sampleTestEntitys, WriteMode.Upsert).Result.ToArray();

            upsertResult.All(r => r.Success).Should().BeTrue();
            upsertResult.All(r => r.Value != Guid.Empty).Should().BeTrue();
            _sampleTestEntitys.All(r => r.Id != Guid.Empty).Should().BeTrue();
            upsertResult.All(r => !r.Messages.Any()).Should().BeTrue();

            foreach (var entity in _sampleTestEntitys)
            {
                T retrieved = _testEntityRepository.GetByIdAsync(entity.Id).Result;

                retrieved.ShouldBeEquivalentTo(entity);
            }
        }

        [Fact]
        public virtual void InsertsNewEntityWithUpsert()
        {
            var sampleEntity = _sampleTestEntitys.First();

            var createResult = _testEntityRepository.SaveAsync(sampleEntity, WriteMode.Upsert).Result;

            createResult.Success.Should().BeTrue();
            createResult.Value.Should().NotBeEmpty();
            sampleEntity.Id.Should().NotBeEmpty();
            createResult.Messages.Should().BeEmpty();

            T retrieved = _testEntityRepository.GetByIdAsync(sampleEntity.Id).Result;

            retrieved.ShouldBeEquivalentTo(sampleEntity);
        }

        [Fact]
        public virtual void FailsToUpdateNonExistingEntity()
        {
            var nonexistentEntity = _fixture.Create<T>();

            var updateResult = _testEntityRepository.SaveAsync(nonexistentEntity, WriteMode.Update).Result;

            updateResult.Success.Should().BeFalse();
            updateResult.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public virtual void FailsToUpdateEntityWithNullId()
        {
            var invalidIdTestEntity = new T { Id = Guid.Empty };
            _testEntityRepository.Invoking(r => r.SaveAsync(invalidIdTestEntity, WriteMode.Update).Wait()).ShouldThrow<InvalidOperationException>();
        }
    }
}
