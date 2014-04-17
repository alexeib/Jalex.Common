using System.Linq;
using Jalex.Infrastructure.Objects;

namespace Jalex.Repository.Extensions
{
    public static class RepositoryExtensions
    {
        public static OperationResult<string> Create<T>(this IInserter<T> repository, T newObject)
        {
            return repository.Create(new[] {newObject}).FirstOrDefault();
        }

        public static OperationResult Delete<T>(this IDeleter<T> repository, string id)
        {
            return repository.Delete(new[] {id}).FirstOrDefault();
        }

        public static T GetById<T>(this IReader<T> repository, string id)
        {
            return repository.GetByIds(new[] {id}).FirstOrDefault();
        }
    }
}
