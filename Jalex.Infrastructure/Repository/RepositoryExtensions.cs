using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public static class RepositoryExtensions
    {
        public static async Task<IEnumerable<OperationResult>> DeleteManyAsync<T>(this IDeleter<T> repository, IEnumerable<Guid> ids)
        {
            var tasks = ids.Select(repository.DeleteAsync)
                           .ToCollection();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result);
        }

        public static async Task<T> GetByIdOrDefaultAsync<T>(this IReader<T> repository, Guid id, T defaultValue)
            where T : class
        {
            return await repository.GetByIdAsync(id).ConfigureAwait(false) ?? defaultValue;
        }

        public static async Task<IEnumerable<T>> GetManyByIdAsync<T>(this IReader<T> repository, IEnumerable<Guid> ids)
            where T : class
        {
            var tasks = ids.Select(repository.GetByIdAsync)
                           .ToCollection();
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results;
        }

        public static async Task<OperationResult<Guid>> SaveAsync<T>(this IWriter<T> writer, T obj, WriteMode writeMode = WriteMode.Upsert)
        {
            return (await writer.SaveManyAsync(new[] { obj }, writeMode).ConfigureAwait(false)).Single();
        }

        public static async Task<OperationResult<Guid>> SaveAsync<T>(this IWriterWithTtl<T> writer, T obj, WriteMode writeMode = WriteMode.Upsert, TimeSpan? timeToLive = null)
        {
            return (await writer.SaveManyAsync(new[] { obj }, writeMode, timeToLive).ConfigureAwait(false)).Single();
        }
    }
}
