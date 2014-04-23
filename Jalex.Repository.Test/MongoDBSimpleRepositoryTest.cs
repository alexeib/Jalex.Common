using System.Configuration;
using Jalex.Repository.MongoDB;

namespace Jalex.Repository.Test
{
    public class MongoDBSimpleRepositoryTest : ISimpleRepositorySpecContainer<MongoDBRepository<TestEntity>>
    {
        static MongoDBSimpleRepositoryTest()
        {
            RepositoryInstance = new MongoDBRepository<TestEntity>
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnectionString"].ConnectionString,
                DatabaseName = ConfigurationManager.AppSettings["MongoDatabase"],
                CollectionName = ConfigurationManager.AppSettings["MongoTestEntityDB"]
            };
        }
    }
}
