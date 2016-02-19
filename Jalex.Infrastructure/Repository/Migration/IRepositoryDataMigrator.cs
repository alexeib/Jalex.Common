using System.Threading.Tasks;

namespace Jalex.Infrastructure.Repository.Migration
{
    public interface IRepositoryDataMigrator
    {
        Task MigrateAsync();
    }
}