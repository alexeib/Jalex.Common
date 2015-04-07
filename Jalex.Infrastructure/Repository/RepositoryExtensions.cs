using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public static class RepositoryExtensions
    {
        public static IEnumerable<OperationResult> DeleteMany<T>(this IDeleter<T> repository, IEnumerable<Guid> ids)
        {
            return ids.Select(repository.Delete).ToArray();
        }

        public static T GetByIdOrDefault<T>(this IReader<T> repository, Guid id)
        {
            T item;
            return repository.TryGetById(id, out item) ? item : default(T);
        }

        public static IEnumerable<T> GetManyByIdOrDefault<T>(this IReader<T> repository, IEnumerable<Guid> ids)
        {
            return ids.Select(repository.GetByIdOrDefault);
        }
    }
}
