using System.Collections.Generic;

namespace Jalex.Infrastructure.Repository
{
    public interface IReader<out T>
    {
        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="ids">the ids of the objects to retrieve</param>
        /// <returns>The requested objects (the ones that weren't found will not be included in the set)</returns>
        IEnumerable<T> GetByIds(IEnumerable<string> ids);

        /// <summary>
        /// Retrieves all objects in the repository
        /// </summary>
        /// <returns>All objects in the repository</returns>
        IEnumerable<T> GetAll();
    }
}
