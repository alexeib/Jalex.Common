using System;
using Jalex.Infrastructure.Repository.Migration;
using Jalex.Repository.Cassandra.Migration;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test.Migration
{
    public class CassandraMigratorTests : IDataMigratorTests
    {
        private const string _pattern = @"aaa";
        private const string _replacement = "bb";

        public CassandraMigratorTests() 
            : base(createFixture(), _pattern, _replacement)
        {
        }

        private static IFixture createFixture()
        {
            var fixture = CassandraRepositoryTests_TestObject.CreateFixture();

            fixture.Register<IDataMigrator>(() => new CassandraRegexMigrator("testobject", new Version(0,1), _pattern, _replacement));

            return fixture;
        }
    }
}
