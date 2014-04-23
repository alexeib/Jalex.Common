using System.Configuration;
using Jalex.Logging;
using Jalex.Logging.Loggers;
using Jalex.Repository.MongoDB;
using MongoDB.Bson;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MongoDBRepositoryTests : IQueryableRepositoryTests
    {
        public MongoDBRepositoryTests()
            : base(createLogger(), createRepository(), createFixture())
        {
            
        }

        private static MemoryLogger createLogger()
        {
            MemoryLogger logger = new MemoryLogger();
            LogManager.OverwriteLogger = logger;

            return logger;
        }

        private static MongoDBRepository<TestEntity> createRepository()
        {
            return new MongoDBRepository<TestEntity>
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnectionString"].ConnectionString,
                DatabaseName = ConfigurationManager.AppSettings["MongoDatabase"],
                CollectionName = ConfigurationManager.AppSettings["MongoTestEntityDB"]
            };
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(() => ObjectId.GenerateNewId().ToString());

            return fixture;
        }
    }
}
