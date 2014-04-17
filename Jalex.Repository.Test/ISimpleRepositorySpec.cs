using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Jalex.Infrastructure.Objects;
using Jalex.Logging;
using Jalex.Logging.Loggers;
using Jalex.Repository.MongoDB;
using Machine.Specifications;

namespace Jalex.Repository.Test
{
    public abstract class ISimpleRepositorySpec
    {
        protected static ISimpleRepository<TestEntity> _testEntityRepository;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;
        protected static MemoryLogger _logger = new MemoryLogger();

        Establish context = () =>
        {
            LogManager.OverwriteLogger = _logger;

            _sampleTestEntitys = new List<TestEntity>
            {
                new TestEntity
                {
                    Name = "TestEntity1",
                    IgnoredProperty = "Some Value"
                },
                new TestEntity
                {
                    Name = "TestEntity2",
                    IgnoredProperty = "Some Value 2"
                }
            };
        };

        private Cleanup after = () =>
        {
            _testEntityRepository.Delete(_sampleTestEntitys.Select(r => r.Id));
            _logger.Clear();
        };
    }

    [Behaviors]
    public class Repository_that_creates_correctly
    {
        protected static ISimpleRepository<TestEntity> _testEntityRepository;
        protected static IEnumerable<OperationResult<string>> _createResult;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;

        It should_be_created_successfully = () => _createResult.All(r => r.Success).ShouldBeTrue();
        It should_have_returned_valid_ids = () => _createResult.All(r => !string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
        It should_have_populated_valid_ids_on_objects = () => _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).ShouldBeTrue();
        It should_have_no_messages = () => _createResult.All(r => !r.Messages.Any()).ShouldBeTrue();
        It should_retrieve_new_TestEntitys = () => _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldNotBeEmpty();
    }

    [Behaviors]
    public class Repository_that_does_not_create_when_entities_exist
    {
        protected static IEnumerable<OperationResult<string>> _createResult;
        protected static MemoryLogger _logger;

        It should_not_be_created_successfully = () => _createResult.All(r => !r.Success).ShouldBeTrue();
        It should_not_have_valid_ids = () => _createResult.All(r => string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
        It should_have_messages = () => _createResult.All(r => r.Messages.Any()).ShouldBeTrue();
        It should_have_logged_errors = () => _logger.Logs.Any().ShouldBeTrue();
    }

    [Behaviors]
    public class Repository_that_throws_exceptions_when_invalid_entity_created
    {
        protected static Exception _exception;
        protected static ISimpleRepository<TestEntity> _testEntityRepository;

        It should_fail = () => _exception.ShouldNotBeNull();
    }

    [Behaviors]
    public class Repository_that_correctly_deletes_entities
    {
        protected static IEnumerable<OperationResult> _deleteResult;
        protected static ISimpleRepository<TestEntity> _testEntityRepository;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;

        It should_be_deleted_successfully = () => _deleteResult.All(r => r.Success).ShouldBeTrue();
        It should_have_no_messages = () => _deleteResult.All(r => !r.Messages.Any()).ShouldBeTrue();
        It should_not_retrieve_deleted_TestEntitys = () => _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldBeEmpty();
    }

    [Behaviors]
    public class Repository_that_fails_to_delete_nonexistant_entities
    {
        protected static IEnumerable<OperationResult> _deleteResult;

        It should_be_not_delete_successfully = () => _deleteResult.All(r => !r.Success).ShouldBeTrue();
    }

    [Behaviors]
    public class Repository_that_correctly_retrieves_entity_by_id
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;
        protected static string _targetTestEntityId;

        private It should_retrieve_one_TestEntity = () => _retrievedTestEntitys.Count().ShouldEqual(1);
        private It should_retrieve_correct_TestEntity = () => _retrievedTestEntitys.All(r => r.Id == _targetTestEntityId).ShouldBeTrue();
        private It should_have_not_retrieved_ignored_properties = () => _retrievedTestEntitys.ShouldEachConformTo(r => string.IsNullOrEmpty(r.IgnoredProperty));
    }

    [Behaviors]
    public class Repository_that_correctly_retrieves_many_entities_by_id
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;

        private It should_retrieve_right_number_of_TestEntitys = () => _retrievedTestEntitys.Count().ShouldEqual(_sampleTestEntitys.Count());
        private It should_retrieve_correct_TestEntitys = () => _retrievedTestEntitys.Select(r => r.Id).Intersect(_sampleTestEntitys.Select(r => r.Id)).Count().ShouldEqual(_sampleTestEntitys.Count());
        private It should_have_not_retrieved_ignored_properties = () => _retrievedTestEntitys.ShouldEachConformTo(r => string.IsNullOrEmpty(r.IgnoredProperty));
    }

    [Behaviors]
    public class Repository_that_does_not_retrieve_nonexistant_entities_by_id
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;

        private It should_retrieve_no_TestEntitys = () => _retrievedTestEntitys.ShouldBeEmpty();
    }



    [Behaviors]
    public class When_Retrieving_TestEntitys_By_Query_For_Non_Existing_Names : ISimpleRepositorySpec
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;

        private Because of = () =>
        {
            _retrievedTestEntitys = _testEntityRepository.Query(r => r.Name == "507f1f77bcf86cd799439012");
        };

        private It should_retrieve_no_TestEntitys = () => _retrievedTestEntitys.ShouldBeEmpty();
    }

    [Subject(typeof(ISimpleRepository<>))]
    public class When_Updating_Existing_TestEntity : ISimpleRepositorySpec
    {
        protected static OperationResult _updateResult;
        protected static TestEntity _TestEntityToUpdate;

        Establish context = () =>
        {
            _testEntityRepository.Create(_sampleTestEntitys);
            _TestEntityToUpdate = _sampleTestEntitys.Last();
            _TestEntityToUpdate.Name = "changed name";
            _TestEntityToUpdate.IgnoredProperty = "changed ignore value";
        };

        Because of = () =>
        {
            _updateResult = _testEntityRepository.Update(_TestEntityToUpdate);
        };

        It should_be_updated_successfully = () => _updateResult.Success.ShouldBeTrue();
        It should_have_no_messages = () => _updateResult.Messages.ShouldBeEmpty();
        It should_have_saved_the_changes = () => _testEntityRepository.GetByIds(new[] { _TestEntityToUpdate.Id }).First().Name.ShouldEqual(_TestEntityToUpdate.Name);
        It should_have_not_saved_ignored_properties = () => _testEntityRepository.GetByIds(new[] { _TestEntityToUpdate.Id }).First().IgnoredProperty.ShouldBeNull();
    }

    [Subject(typeof(ISimpleRepository<>))]
    public class When_Updating_Non_Existing_TestEntity : ISimpleRepositorySpec
    {
        protected static OperationResult _updateResult;
        protected static TestEntity _nonExistentTestEntity;

        private Establish context = () => _nonExistentTestEntity = new TestEntity { Id = "507f1f77bcf86cd799439011" };

        Because of = () =>
        {
            _updateResult = _testEntityRepository.Update(_nonExistentTestEntity);
        };

        It should_be_not_update_successfully = () => _updateResult.Success.ShouldBeFalse();
        It should_have_messages = () => _updateResult.Messages.ShouldNotBeEmpty();
    }

    [Subject(typeof(ISimpleRepository<>))]
    public class When_Updating_TestEntity_With_Null_Id : ISimpleRepositorySpec
    {
        protected static TestEntity _invalidIdTestEntity;
        protected static Exception _exception;

        private Establish context = () => _invalidIdTestEntity = new TestEntity { Id = null };

        Because of = () =>
        {
            _exception = Catch.Exception(() => _testEntityRepository.Update(_invalidIdTestEntity));
        };

        It should_throw_exception = () => _exception.ShouldNotBeNull();
    }
}