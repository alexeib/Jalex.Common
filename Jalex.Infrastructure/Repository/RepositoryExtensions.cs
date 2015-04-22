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
            return repository.GetByIdOrDefault(id, default(T));
        }

        public static T GetByIdOrDefault<T>(this IReader<T> repository, Guid id, T defaultValue)
        {
            T item;
            return repository.TryGetById(id, out item) ? item : defaultValue;
        }

        public static IEnumerable<T> GetManyByIdOrDefault<T>(this IReader<T> repository, IEnumerable<Guid> ids)
        {
            return ids.Select(repository.GetByIdOrDefault);
        }
    }
}
