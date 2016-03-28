using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cassandra;
using Jalex.Repository.Migration;

namespace Jalex.Repository.Cassandra.Migration
{
    public class CassandraRegexMigrator : ITableDataMigrator
    {
        private readonly string _pattern;
        private readonly string _replacement;

        public string TargetTable { get; }
        public int TargetVersion { get; }

        public CassandraRegexMigrator(string targetTable, int targetVersion, string pattern, string replacement)
        {
            if (targetTable == null) throw new ArgumentNullException(nameof(targetTable));
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

                string valueAsString;

                var valueAsDict = originalValue as IDictionary;
                var valueAsList = originalValue as IList;

                if (valueAsDict != null)
                {
                    var dictValues = (from object key in valueAsDict.Keys
                                      select toCassandraStr(key) + ":" + toCassandraStr(valueAsDict[key]));
                    valueAsString = "{" + string.Join(",", dictValues) + "}";
                }
                else if (valueAsList != null)
                {
                    var listValues = (from object item in valueAsList select toCassandraStr(item));
                    valueAsString = "[" + string.Join(",", listValues) + "]";
                }
                else if (column.Type == typeof(DateTimeOffset))
                {
                    if(((DateTimeOffset)originalValue).Year == 1)
                        continue;

                    valueAsString = toCassandraStr(originalValue);
                }
                else
                {
                    valueAsString = Convert.ToString(originalValue);
                }

                var modifiedValue = Regex.Replace(valueAsString, _pattern, _replacement);
                columns.Add(colName);
                if (originalValue is string || originalValue is DateTimeOffset)
                {
                    modifiedValue = toCassandraStr(modifiedValue);
                }
                values.Add(modifiedValue);
            }

            var insertQuery = $"insert into {TargetTable} ( {string.Join(",", columns)} ) values ({string.Join(",", values)})";
            var statement = new SimpleStatement(insertQuery);
            await session.ExecuteAsync(statement).ConfigureAwait(false);
        }

        private static string toCassandraStr(object item)
        {
            if (item is string)
            {
                return "'" + ((string)item).Replace("'", "''") + "'";
            }
            if (item is DateTimeOffset)
            {
                return ((DateTimeOffset) item).ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
            return item?.ToString();
        }
    }
}
