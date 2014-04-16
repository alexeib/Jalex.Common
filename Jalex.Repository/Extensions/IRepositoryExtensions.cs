using System.Linq;
using Jalex.Infrastructure.Objects;

namespace Jalex.Repository.Extensions
{
    public static class IRepositoryExtensions
    {
        public static OperationResult<string> Create<T>(this ISimpleRepository<T> repository, T newObject)
        {
            return repository.Create(new[] {newObject}).FirstOrDefault();
        }

        public static OperationResult Delete<T>(this ISimpleRepository<T> repository, string id)
        {
            return repository.Delete(new[] {id}).FirstOrDefault();
        }

        public static T GetById<T>(this ISimpleRepository<T> repository, string id)
        {
            return repository.GetByIds(new[] {id}).FirstOrDefault();
        }
    }
}
