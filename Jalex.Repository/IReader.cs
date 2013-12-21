using System;
using System.Collections.Generic;

namespace Jalex.Repository
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
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        IEnumerable<T> Query(Func<T, bool> query);
    }
}
