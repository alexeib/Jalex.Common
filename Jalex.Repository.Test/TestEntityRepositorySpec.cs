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
    public abstract class TestEntityRepositorySpec
    {
        protected static IRepository<TestEntity> _testEntityRepository;
        protected static IEnumerable<TestEntity> _sampleTestEntitys;
        protected static MemoryLogger _logger = new MemoryLogger();

        Establish context = () =>
        {
            LogManager.OverwriteLogger = _logger;

            _testEntityRepository = new MongoDBRepository<TestEntity>
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnectionString"].ConnectionString,
                DatabaseName = ConfigurationManager.AppSettings["MongoDatabase"],
                CollectionName = ConfigurationManager.AppSettings["MongoTestEntityDB"]
            };

            _sampleTestEntitys = new List<TestEntity>
            {
                new TestEntity
                {
                    Name = "TestEntity1",
                },
                new TestEntity
                {
                    Name = "TestEntity2",
                }
            };
        };

        private Cleanup after = () =>
        {
            _testEntityRepository.Delete(_sampleTestEntitys.Select(r => r.Id));
            _logger.Clear();
        };
    }

    [Subject(typeof(IRepository<>))]
    public class When_Creating_TestEntitys : TestEntityRepositorySpec
    {
        protected static IEnumerable<OperationResult<string>> _createResult;

        Because of = () =>
        {
            _createResult = _testEntityRepository.Create(_sampleTestEntitys);
        };

        It should_be_created_successfully = () => _createResult.All(r => r.Success).ShouldBeTrue();
        It should_have_returned_valid_ids = () => _createResult.All(r => !string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
        It should_have_populated_valid_ids_on_objects = () => _sampleTestEntitys.All(r => !string.IsNullOrEmpty(r.Id)).ShouldBeTrue();
        It should_have_no_messages = () => _createResult.All(r => !r.Messages.Any()).ShouldBeTrue();
        It should_retrieve_new_TestEntitys = () => _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldNotBeEmpty();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Creating_Existing_TestEntitys : TestEntityRepositorySpec
    {
        protected static IEnumerable<OperationResult<string>> _createResult;

        Establish context = () => _testEntityRepository.Create(_sampleTestEntitys);

        Because of = () =>
        {
            _createResult = _testEntityRepository.Create(_sampleTestEntitys);
        };

        It should_not_be_created_successfully = () => _createResult.All(r => !r.Success).ShouldBeTrue();
        It should_not_have_valid_ids = () => _createResult.All(r => string.IsNullOrEmpty(r.Value)).ShouldBeTrue();
        It should_have_messages = () => _createResult.All(r => r.Messages.Any()).ShouldBeTrue();
        It should_have_logged_errors = () => _logger.Logs.Any().ShouldBeTrue();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Creating_TestEntity_With_Invalid_Id : TestEntityRepositorySpec
    {
        protected static Exception Exception;

        Because of = () =>
        {
            Exception = Catch.Exception(() => _testEntityRepository.Create(new[] { new TestEntity { Id = "FakeId", Name = "FakeName" } }));
        };

        It should_fail = () => Exception.ShouldNotBeNull();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Deleting_Existing_TestEntity : TestEntityRepositorySpec
    {
        protected static IEnumerable<OperationResult> _deleteResult;

        Establish context = () => _testEntityRepository.Create(_sampleTestEntitys);

        Because of = () =>
        {
            _deleteResult = _testEntityRepository.Delete(_sampleTestEntitys.Select(r => r.Id));
        };

        It should_be_deleted_successfully = () => _deleteResult.All(r => r.Success).ShouldBeTrue();
        It should_have_no_messages = () => _deleteResult.All(r => !r.Messages.Any()).ShouldBeTrue();
        It should_not_retrieve_deleted_TestEntitys = () => _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id)).ShouldBeEmpty();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Deleting_Non_Existing_TestEntity : TestEntityRepositorySpec
    {
        protected static IEnumerable<OperationResult> _deleteResult;

        Because of = () =>
        {
            _deleteResult = _testEntityRepository.Delete(new[] { "507f1f77bcf86cd799439011", "507f1f77bcf86cd799439012" });
        };

        It should_be_not_delete_successfully = () => _deleteResult.All(r => !r.Success).ShouldBeTrue();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Retrieving_One_TestEntity_By_Id : TestEntityRepositorySpec
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;
        protected static string _targetTestEntityId;

        private Establish context = () =>
        {
            _testEntityRepository.Create(_sampleTestEntitys);
            _targetTestEntityId = _sampleTestEntitys.First().Id;
        };

        private Because of = () =>
        {
            _retrievedTestEntitys = _testEntityRepository.GetByIds(new[] { _targetTestEntityId });
        };

        private It should_retrieve_one_TestEntity = () => _retrievedTestEntitys.Count().ShouldEqual(1);
        private It should_retrieve_correct_TestEntity = () => _retrievedTestEntitys.All(r => r.Id == _targetTestEntityId).ShouldBeTrue();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Retrieving_Several_TestEntitys_By_Id : TestEntityRepositorySpec
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;

        private Establish context = () => _testEntityRepository.Create(_sampleTestEntitys);

        private Because of = () =>
        {
            _retrievedTestEntitys = _testEntityRepository.GetByIds(_sampleTestEntitys.Select(r => r.Id));
        };

        private It should_retrieve_right_number_of_TestEntitys = () => _retrievedTestEntitys.Count().ShouldEqual(_sampleTestEntitys.Count());
        private It should_retrieve_correct_TestEntitys = () => _retrievedTestEntitys.Select(r => r.Id).Intersect(_sampleTestEntitys.Select(r => r.Id)).Count().ShouldEqual(_sampleTestEntitys.Count());
    }

    [Subject(typeof(IRepository<>))]
    public class When_Retrieving_TestEntitys_By_Non_Existant_Id : TestEntityRepositorySpec
    {
        protected static IEnumerable<TestEntity> _retrievedTestEntitys;

        private Because of = () =>
        {
            _retrievedTestEntitys = _testEntityRepository.GetByIds(new[] { "507f1f77bcf86cd799439011", "507f1f77bcf86cd799439012" });
        };

        private It should_retrieve_no_TestEntitys = () => _retrievedTestEntitys.ShouldBeEmpty();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Updating_Existing_TestEntity : TestEntityRepositorySpec
    {
        protected static OperationResult _updateResult;
        protected static TestEntity _TestEntityToUpdate;

        Establish context = () =>
        {
            _testEntityRepository.Create(_sampleTestEntitys);
            _TestEntityToUpdate = _sampleTestEntitys.Last();
            _TestEntityToUpdate.Name = "changed name";
        };

        Because of = () =>
        {
            _updateResult = _testEntityRepository.Update(_TestEntityToUpdate);
        };

        It should_be_updated_successfully = () => _updateResult.Success.ShouldBeTrue();
        It should_have_no_messages = () => _updateResult.Messages.ShouldBeEmpty();
        It should_have_saved_the_changes = () => _testEntityRepository.GetByIds(new[] {_TestEntityToUpdate.Id}).First().Name.ShouldEqual(_TestEntityToUpdate.Name);
    }

    [Subject(typeof(IRepository<>))]
    public class When_Updating_Non_Existing_TestEntity : TestEntityRepositorySpec
    {
        protected static OperationResult _updateResult;
        protected static TestEntity _nonExistentTestEntity;

        private Establish context = () => _nonExistentTestEntity = new TestEntity {Id = "507f1f77bcf86cd799439011"};

        Because of = () =>
        {
            _updateResult = _testEntityRepository.Update(_nonExistentTestEntity);
        };

        It should_be_not_update_successfully = () => _updateResult.Success.ShouldBeFalse();
        It should_have_messages = () => _updateResult.Messages.ShouldNotBeEmpty();
    }

    [Subject(typeof(IRepository<>))]
    public class When_Updating_TestEntity_With_Null_Id : TestEntityRepositorySpec
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