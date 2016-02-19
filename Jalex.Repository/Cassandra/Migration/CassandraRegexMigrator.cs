using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cassandra;
using Jalex.Infrastructure.Repository.Migration;

namespace Jalex.Repository.Cassandra.Migration
{
    public class CassandraRegexMigrator : ITableDataMigrator
    {
        private readonly string _pattern;
        private readonly string _replacement;

        public string TargetTable { get; }
        public Version TargetVersion { get; }

        public CassandraRegexMigrator(string targetTable, Version targetVersion, string pattern, string replacement)
        {
            if (targetTable == null) throw new ArgumentNullException(nameof(targetTable));
            if (targetVersion == null) throw new ArgumentNullException(nameof(targetVersion));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));

            TargetTable = targetTable.Replace(";", "");
            TargetVersion = targetVersion;
            _pattern = pattern;
            _replacement = replacement;
        }

        public async Task ExecuteAsync()
        {
            var session = CassandraSessionPool.GetSession();

            var statement = new SimpleStatement($"select * from {TargetTable}");
            var rowSet = await session.ExecuteAsync(statement)
                                      .ConfigureAwait(false);

            foreach (var row in rowSet)
            {
                await updateRowAsync(session, row, rowSet.Columns)
                    .ConfigureAwait(false);
            }
        }

        private async Task updateRowAsync(ISession session, Row row, CqlColumn[] cqlColumns)
        {
            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            foreach (var column in cqlColumns)
            {
                var colName = column.Name;

                var originalValue = row.GetValue<object>(colName);
                if (originalValue == null)
                {
                    continue;
                }

                var valueAsString = Convert.ToString(originalValue);

                var modifiedValue = Regex.Replace(valueAsString, _pattern, _replacement);
                columns.Add(colName);
                if (originalValue is string)
                {
                    modifiedValue = "'" + modifiedValue + "'";
                }
                values.Add(modifiedValue);
            }

            var insertQuery = $"insert into {TargetTable} ( {string.Join(",", columns)} ) values ({string.Join(",", values)})";
            var statement = new SimpleStatement(insertQuery);
            await session.ExecuteAsync(statement).ConfigureAwait(false);
        }
    }
}
