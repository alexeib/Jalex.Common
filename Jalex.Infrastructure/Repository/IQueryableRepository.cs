﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IQueryableRepository<T> : ISimpleRepository<T>, IQueryableReader<T>
        where T : class
    {

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression);
    }

    public interface IQueryableRepositoryWithTtl<T> : IQueryableRepository<T>, IWriterWithTtl<T>
        where T : class
    { }
}

