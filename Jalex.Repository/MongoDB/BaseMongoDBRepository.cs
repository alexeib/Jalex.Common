using System.Configuration;
using Jalex.Logging;
using MongoDB.Driver;

namespace Jalex.Repository.MongoDB
{
    public abstract class BaseMongoDBRepository
    {        
        private const string _defaultConnectionStringName = "mongo-default";
        private const string _defaultDatabaseSettingName = "mongo-db";       

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        protected MongoDatabase getMongoDatabase()
        {
            string connectionString = ConnectionString ?? ConfigurationManager.ConnectionStrings[_defaultConnectionStringName].ConnectionString;
            string databaseName = DatabaseName ?? ConfigurationManager.AppSettings[_defaultDatabaseSettingName];

            var mongoClient = new MongoClient(connectionString);
            var mongoServer = mongoClient.GetServer();
            var db = mongoServer.GetDatabase(databaseName);
            return db;
        }
    }
}
