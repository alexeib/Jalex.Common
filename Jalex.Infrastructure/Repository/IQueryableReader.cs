using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.Repository
{
    public interface IQueryableReader<T>
    {
        /// <summary>
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        IEnumerable<T> Query(Expression<Func<T, bool>> query);

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        T FirstOrDefault(Expression<Func<T, bool>> query);
    }
}
