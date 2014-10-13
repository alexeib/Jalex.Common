using System.Collections.Generic;

namespace Jalex.Infrastructure.Repository
{
    public interface IReader<T>
    {
        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <param name="obj"> retrieved object</param>
        /// <returns>True if retrieval succeeded, false otherwise</returns>
        bool TryGetById(string id, out T obj);

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        IEnumerable<T> GetAll();
    }
}
