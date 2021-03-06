﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Jalex.Infrastructure.Repository
{
    public interface IQueryableReader<T> : IReader<T> 
        where T : class
    {
        /// <summary>
        /// Returns objects stored in the repository that satisfy a given query. 
        /// </summary>
        /// <param name="query">The query that must be satisfied to include an object in the resulting parameter list</param>
        /// <returns>Objects in the repository that satisfy the query</returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query);

        /// <summary>
        /// Projects a subset of an object that satisfy the given query
        /// </summary>
        Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> query);

        /// <summary>
        /// Projects a subset of all objects in the repository
        /// </summary>
        Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection);

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query);
    }
}
