using System.Collections.Generic;
using System.Linq;

namespace Jalex.Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        public static T[] ToArrayEfficient<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as T[] ?? enumerable.ToArray();
        }
    }
}
