using System.Configuration;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Jalex.Repository.MongoDB;
using Jalex.Repository.Utils;
using MongoDB.Bson;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class MongoDBRepositoryTests : IQueryableRepositoryTests
    {
        public MongoDBRepositoryTests()
            : base(createFixture())
        {

        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Register<IIdProvider>(fixture.Create<ObjectIdIdProvider>);
            fixture.Register<IReflectedTypeDescriptorProvider>(fixture.Create<ReflectedTypeDescriptorProvider>);
            fixture.Register<IQueryableRepository<TestObject>>(() =>
                                                               {
                                                                   var repo = fixture.Create<MongoDBRepository<TestObject>>();
                                                                   repo.ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnectionString"].ConnectionString;
                                                                   repo.DatabaseName = ConfigurationManager.AppSettings["MongoDatabase"];
                                                                   repo.CollectionName = ConfigurationManager.AppSettings["MongoTestEntityDB"];
                                                                   return repo;
                                                               });
            fixture.Register<ISimpleRepository<TestObject>>(fixture.Create<IQueryableRepository<TestObject>>);

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(() => ObjectId.GenerateNewId().ToString());

            return fixture;
        }
    }
}
