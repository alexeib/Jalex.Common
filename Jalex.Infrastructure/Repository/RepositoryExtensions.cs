using System.Linq;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
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

        /// <summary>
        /// Updates an existing object. This will not create a new object. Note that this update does a full replace
        /// </summary>
        /// <typeparam name="T">The type of object to update</typeparam>
        /// <param name="repository">The repository to use for updating</param>
        /// <param name="objectToUpdate">The object to update</param>
        /// <returns></returns>
        public static OperationResult Update<T>(this IUpdater<T> repository, T objectToUpdate)
        {
            var result = repository.Update(new[] {objectToUpdate}).FirstOrDefault();
            return result;
        }
    }
}
