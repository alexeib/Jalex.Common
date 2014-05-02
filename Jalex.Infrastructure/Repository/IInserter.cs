using System.Collections.Generic;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IInserter<in T>
    {
        /// <summary>
        /// Creates new objects. This will not update existing objects
        /// </summary>
        /// <param name="newObjects">objects to create</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        IEnumerable<OperationResult<string>> Create(IEnumerable<T> newObjects);
    }
}
