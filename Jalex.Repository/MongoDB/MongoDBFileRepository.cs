using System;
using System.IO;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using NLog;

namespace Jalex.Repository.MongoDB
{
    public class MongoDBFileRepository : IFileRepository
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private bool _indicesEnsured;
        private readonly MongoHelper _helper = new MongoHelper();

        public string ConnectionString
        {
            get { return _helper.ConnectionString; }
            set { _helper.ConnectionString = value; }
        }

        public string DatabaseName
        {
            get { return _helper.DatabaseName; }
            set { _helper.DatabaseName = value; }
        }

        public OperationResult<string> Create(string fileName, Stream fileStream)
        {
            var fs = getGridFS();

            try
            {
                var fileInfo = fs.Upload(fileStream, fileName);
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                return new OperationResult<string>(true, fileInfo.Id.ToString());
            }
            catch (Exception wce)
            {
                _logger.Error(wce, "Error when creating file " + fileName);
                return new OperationResult<string>
                {
                    Success = false,
                    Value = null,
                    Messages = new[]
                    {
                        new Message(Severity.Error, string.Format("Failed to create file {0}", fileName))
                    }
                };
            }
        }

        public OperationResult<Stream> GetById(string id)
        {
            var fs = getGridFS();
            var objectId = new ObjectId(id);
            var fileInfo = fs.FindOneById(objectId);
            return getFileStreamFromInfo(fileInfo);
        }

        public OperationResult<Stream> GetByFileName(string fileName)
        {
            var fs = getGridFS();
            var fileInfo = fs.FindOne(fileName);
            return getFileStreamFromInfo(fileInfo);
        }

        public OperationResult DeleteById(string id)
        {
            var fs = getGridFS();
            var objectId = new ObjectId(id);
            fs.DeleteById(objectId);
            return new OperationResult(true);
        }

        public OperationResult DeleteByFileName(string fileName)
        {
            var fs = getGridFS();
            fs.Delete(fileName);
            return new OperationResult(true);
        }

        private MongoGridFS getGridFS()
        {
            var client = _helper.GetMongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase(DatabaseName);
            var fs = db.GridFS;

            if (!_indicesEnsured)
            {
                ensureIndices(fs);
                _indicesEnsured = true;
            }
            
            return fs;
        }

        private void ensureIndices(MongoGridFS fs)
        {

            var fileNameIndex = new IndexKeysBuilder().Ascending("filename");
            fs.Files.CreateIndex(fileNameIndex, IndexOptions.SetUnique(true));
        }

        private OperationResult<Stream> getFileStreamFromInfo(MongoGridFSFileInfo fileInfo)
        {
            OperationResult<Stream> result = new OperationResult<Stream>();
            if (fileInfo == null)
            {
                result.Success = false;
                result.Messages = new[] {new Message(Severity.Error, "File not found")};
            }
            else
            {
                result.Success = true;
                result.Value = fileInfo.OpenRead();
            }

            return result;
        }
    }
}
