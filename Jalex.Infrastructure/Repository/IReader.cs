using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jalex.Infrastructure.Repository
{
    public interface IReader<T>
        where T: class
    {
        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="id">the id of the objects to retrieve</param>
        /// <returns>The item being retrieved or default(T) if it was not found</returns>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        Task<IEnumerable<T>> GetAllAsync();
    }
}
