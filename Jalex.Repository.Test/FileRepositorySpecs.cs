using System.Configuration;
using System.IO;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Logging;
using Jalex.Logging.Loggers;
using Jalex.Repository.MongoDB;
using Machine.Specifications;

namespace Jalex.Repository.Test
{
    [Subject(typeof(IFileRepository))]
    public abstract class FileRepositorySpecs
    {
        protected static IFileRepository _fileRepository;
        protected static string _testFileName;
        protected static MemoryLogger _logger = new MemoryLogger();

        Establish context = () =>
        {
            LogManager.DefaultLogger = _logger;

            _fileRepository = new MongoDBFileRepository
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnectionString"].ConnectionString,
                DatabaseName = ConfigurationManager.AppSettings["MongoDatabase"]
            };

            _testFileName = ConfigurationManager.AppSettings["TestFileName"];
        };

        private Cleanup after = () =>
        {
            _fileRepository.DeleteByFileName(_testFileName);
            _logger.Clear();
        };
    }

    public abstract class SavedFileRepositorySpecs : FileRepositorySpecs
    {
        protected static OperationResult<string> _createResult;
        protected static string _fileContents;

        Establish context = () =>
        {
            using (var tempStream = File.OpenRead(_testFileName))
            {
                _createResult = _fileRepository.Create(_testFileName, tempStream);
                _fileContents = tempStream.ReadToEndAndClose();
            }
        };

        Cleanup after = () => _fileRepository.DeleteByFileName(_testFileName);
    }

    [Subject(typeof(IFileRepository))]
    public class When_Creating_A_Valid_File : FileRepositorySpecs
    {
        protected static Stream _streamToSave;
        protected static OperationResult<string> _result;

        Establish context = () => _streamToSave = File.OpenRead(_testFileName);

        Because of = () => _result = _fileRepository.Create(_testFileName, _streamToSave);

        It should_be_successful = () => _result.Success.ShouldBeTrue();
        It should_not_have_messages = () => _result.Messages.ShouldBeEmpty();
        It should_have_a_valid_id = () => _result.Value.ShouldNotBeEmpty();

        Cleanup after = () =>
        {
            _streamToSave.Dispose();
            _fileRepository.DeleteByFileName(_testFileName);
        };
    }

    [Subject(typeof(IFileRepository))]
    public class When_Creating_A_File_With_Same_Name : SavedFileRepositorySpecs
    {
        protected static Stream _streamToSave;
        protected static OperationResult<string> _result;

        Establish context = () =>
        {
            _streamToSave = File.OpenRead(_testFileName);
        };

        Because of = () => _result = _fileRepository.Create(_testFileName, _streamToSave);

        It should_not_be_successful = () => _result.Success.ShouldBeFalse();
        It should_have_messages = () => _result.Messages.ShouldNotBeEmpty();
        It should_not_have_a_valid_id = () => _result.Value.ShouldBeNull();
        It should_have_logged_errors = () => _logger.Logs.ShouldNotBeEmpty();

        Cleanup after = () => _streamToSave.Dispose();
    }

    [Subject(typeof(IFileRepository))]
    public class When_Getting_A_Valid_File_By_Id : SavedFileRepositorySpecs
    {
        protected static OperationResult<Stream> _getResult;

        Because of = () => _getResult = _fileRepository.GetById(_createResult.Value);

        It should_be_successful = () => _getResult.Success.ShouldBeTrue();
        It should_not_have_messages = () => _getResult.Messages.ShouldBeEmpty();
        It should_have_non_empty_stream = () => _getResult.Value.ShouldNotBeNull();
        It should_have_the_same_contents_as_what_was_Saved = () => _getResult.Value.ReadToEndAndClose().ShouldEqual(_fileContents);
    }

    [Subject(typeof(IFileRepository))]
    public class When_Getting_A_File_By_Invalid_Id : FileRepositorySpecs
    {
        protected static OperationResult<Stream> _getResult;

        Because of = () => _getResult = _fileRepository.GetById("526421700d28cb1d64facbe1");

        It should_be_successful = () => _getResult.Success.ShouldBeFalse();
        It should_not_have_messages = () => _getResult.Messages.ShouldNotBeEmpty();
        It should_have_non_empty_stream = () => _getResult.Value.ShouldBeNull();
    }

    [Subject(typeof(IFileRepository))]
    public class When_Getting_A_Valid_File_By_Name : SavedFileRepositorySpecs
    {
        protected static OperationResult<Stream> _getResult;

        Because of = () => _getResult = _fileRepository.GetByFileName(_testFileName);

        It should_be_successful = () => _getResult.Success.ShouldBeTrue();
        It should_not_have_messages = () => _getResult.Messages.ShouldBeEmpty();
        It should_have_non_empty_stream = () => _getResult.Value.ShouldNotBeNull();
        It should_have_the_same_contents_as_what_was_Saved = () => _getResult.Value.ReadToEndAndClose().ShouldEqual(_fileContents);
    }

    [Subject(typeof(IFileRepository))]
    public class When_Getting_A_File_By_Invalid_Name : FileRepositorySpecs
    {
        protected static OperationResult<Stream> _getResult;

        Because of = () => _getResult = _fileRepository.GetByFileName("name that does not exist");

        It should_be_successful = () => _getResult.Success.ShouldBeFalse();
        It should_not_have_messages = () => _getResult.Messages.ShouldNotBeEmpty();
        It should_have_non_empty_stream = () => _getResult.Value.ShouldBeNull();
    }

    [Subject(typeof(IFileRepository))]
    public class When_Deleting_A_Valid_File_By_Id : SavedFileRepositorySpecs
    {
        protected static OperationResult _deleteResult;

        Because of = () => _deleteResult = _fileRepository.DeleteById(_createResult.Value);

        It should_be_successful = () => _deleteResult.Success.ShouldBeTrue();
        It should_not_have_messages = () => _deleteResult.Messages.ShouldBeEmpty();
        It should_not_exist_in_the_repository = () => _fileRepository.GetById(_createResult.Value).Success.ShouldBeFalse();
    }

    [Subject(typeof(IFileRepository))]
    public class When_Deleting_A_Valid_File_By_Name : SavedFileRepositorySpecs
    {
        protected static OperationResult _deleteResult;

        Because of = () => _deleteResult = _fileRepository.DeleteByFileName(_testFileName);

        It should_be_successful = () => _deleteResult.Success.ShouldBeTrue();
        It should_not_have_messages = () => _deleteResult.Messages.ShouldBeEmpty();
        It should_not_exist_in_the_repository = () => _fileRepository.GetByFileName(_testFileName).Success.ShouldBeFalse();
    }
}