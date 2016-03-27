using System;
using Jalex.Repository.Cassandra.Migration;
using Jalex.Repository.Migration;
using Ploeh.AutoFixture;

namespace Jalex.Repository.Test.Migration
{
    public class CassandraMigratorTests : TableDataMigratorTests
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

            fixture.Register<ITableDataMigrator>(() => new CassandraRegexMigrator("testobject", new Version(0,1), _pattern, _replacement));

            return fixture;
        }
    }
}
