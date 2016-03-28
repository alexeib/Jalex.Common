using System.Threading.Tasks;

namespace Jalex.Repository.Migration
{
    public interface ITableDataMigrator
    {
        string TargetTable { get; }

        int TargetVersion { get; }

        Task ExecuteAsync();
    }
}