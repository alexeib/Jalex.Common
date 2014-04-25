using System;
using System.Configuration;
using Jalex.Logging;
using MongoDB.Driver;

namespace Jalex.Repository.MongoDB
{
    public abstract class BaseMongoDBRepository : BaseRepository
    {        
        private const string _defaultConnectionStringName = "mongo-default";
        private const string _defaultDatabaseSettingName = "mongo-db";       

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        protected MongoDatabase getMongoDatabase()
        {
            string connectionString = ConnectionString ?? ConfigurationManager.ConnectionStrings[_defaultConnectionStringName].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Must specify MongoDB connection string by providing a value in the ConnectionString property or populating the " + _defaultConnectionStringName + " connection string setting in config file");
            }
            
            string databaseName = DatabaseName ?? ConfigurationManager.AppSettings[_defaultDatabaseSettingName];

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("Must specify MongoDB database name by providing a value in the DatabaseName property or populating the " + _defaultDatabaseSettingName + " app setting");
            }

            var mongoClient = new MongoClient(connectionString);
            var mongoServer = mongoClient.GetServer();
            var db = mongoServer.GetDatabase(databaseName);
            return db;
        }
    }
}
