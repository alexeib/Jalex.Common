using System.Collections.Generic;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IUpdater<in T>
    {
        /// <summary>
        /// Updates many existing objects. This will not create a new object. Note that this update does a full replace
        /// </summary>
        /// <param name="objectsToUpdate">the objects to update</param>
        /// <returns>results of update for each objects (same order)</returns>
        IEnumerable<OperationResult> Update(IEnumerable<T> objectsToUpdate);
    }
}
