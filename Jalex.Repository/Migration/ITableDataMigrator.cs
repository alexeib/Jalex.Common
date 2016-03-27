using System;
using System.Threading.Tasks;

namespace Jalex.Repository.Migration
{
    public interface ITableDataMigrator
    {
        string TargetTable { get; }

        Version TargetVersion { get; }

        Task ExecuteAsync();
    }
}