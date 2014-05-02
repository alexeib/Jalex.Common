using System.Collections.Generic;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IDeleter<T>
    {
        /// <summary>
        /// Deletes an existing objects
        /// </summary>
        /// <param name="ids">The ids of the objects to delete</param>
        /// <returns>the result of the delete operation</returns>
        IEnumerable<OperationResult> Delete(IEnumerable<string> ids);
    }
}
