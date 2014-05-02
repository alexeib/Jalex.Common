using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.Repository
{
    public interface IQueryable<T>
    {
        /// <summary>
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        IEnumerable<T> Query(Expression<Func<T, bool>> query);
    }
}
