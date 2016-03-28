using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Migration
{
    public class RepositoryDataMigrator : IRepositoryDataMigrator
    {
        private readonly IEnumerable<ITableDataMigrator> _tableDataMigrators;
        private readonly IQueryableRepository<TableVersion> _tableVersionRepository;

        public RepositoryDataMigrator(IEnumerable<ITableDataMigrator> tableDataMigrators, IQueryableRepository<TableVersion> tableVersionRepository)
        {
            if (tableDataMigrators == null) throw new ArgumentNullException(nameof(tableDataMigrators));
            if (tableVersionRepository == null) throw new ArgumentNullException(nameof(tableVersionRepository));

            _tableDataMigrators = tableDataMigrators;
            _tableVersionRepository = tableVersionRepository;
        }

        public async Task MigrateAsync()
        {
            foreach (var migratorsByTable in _tableDataMigrators.GroupBy(d => d.TargetTable))
            {
                var currVersion = await _tableVersionRepository.FirstOrDefaultAsync(tv => tv.TableName == migratorsByTable.Key)
                                                               .ConfigureAwait(false) ?? new TableVersion { TableName = migratorsByTable.Key };
                var migrators = migratorsByTable.Where(m => m.TargetVersion > currVersion.Version)
                                                .OrderBy(m => m.TargetVersion);
                try
                {
                    foreach (var migrator in migrators)
                    {
                        await migrator.ExecuteAsync();
                        currVersion.Version = migrator.TargetVersion;
                    }
                }
                finally
                {
                    await _tableVersionRepository.SaveAsync(currVersion);
                }

            }
        }
    }
}
