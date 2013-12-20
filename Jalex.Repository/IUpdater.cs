using Jalex.Infrastructure.Objects;

namespace Jalex.Repository
{
    public interface IUpdater<in T>
    {
        /// <summary>
        /// Updates an existing object. This will not create a new object. Note that this update does a full replace
        /// </summary>
        /// <param name="objectToUpdate">object to update</param>
        /// <returns>the result of the update operation</returns>
        OperationResult Update(T objectToUpdate);
    }
}
