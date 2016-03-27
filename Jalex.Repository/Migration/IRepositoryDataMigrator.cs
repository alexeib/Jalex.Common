using System.Threading.Tasks;

namespace Jalex.Repository.Migration
{
    public interface IRepositoryDataMigrator
    {
        Task MigrateAsync();
    }
}