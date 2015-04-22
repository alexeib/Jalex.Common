using System;
using System.Linq.Expressions;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IQueryableRepository<T> : ISimpleRepository<T>, IQueryable<T>
    {

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        OperationResult DeleteWhere(Expression<Func<T, bool>> expression);
    }
}
