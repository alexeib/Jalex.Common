using System;
using System.Threading.Tasks;

namespace Jalex.Infrastructure.Repository.Migration
{
    public interface ITableDataMigrator
    {
        string TargetTable { get; }

        Version TargetVersion { get; }

        Task ExecuteAsync();
    }
}