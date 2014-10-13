using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jalex.Services.Caching
{
    public interface IIndexCache<T>
    {
        IEnumerable<string> IndexedProperties { get; }

        void Index(T obj);
        void DeIndex(T obj);
        string FindIdByQuery(Expression<Func<T, bool>> query);
        void DeIndexByQuery(Expression<Func<T, bool>> query);
    }
}