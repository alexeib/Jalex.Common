using System.Collections.Generic;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IDeleter<T>
    {
        /// <summary>
        /// Deletes an existing object
        /// </summary>
        /// <param name="id">The id of the object to delete</param>
        /// <returns>the result of the delete operation</returns>
        OperationResult Delete(string id);
    }
}
