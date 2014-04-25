using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jalex.Repository.Cassandra;
using Jalex.Repository.MongoDB;
using MongoDB.Bson;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test
{
    public class CassandraRepositoryTests : IQueryableRepositoryTests
    {
        public CassandraRepositoryTests()
            : base(createRepository(), createFixture())
        {
            
        }

        private static CassandraRepository<TestEntity> createRepository()
        {
            var cassandraIdGenerator = new DefaultCassandraIdProvider();
            return new CassandraRepository<TestEntity>(cassandraIdGenerator);
        }

        private static IFixture createFixture()
        {
            IFixture fixture = new Fixture();

            DefaultCassandraIdProvider idProvider = new DefaultCassandraIdProvider();

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            fixture.Register<string>(idProvider.GenerateNewId);

            return fixture;
        }
    }
}
