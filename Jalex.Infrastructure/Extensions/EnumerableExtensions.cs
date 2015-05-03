using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jalex.Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArrayEfficient<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as T[] ?? enumerable.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyCollection<T> ToCollection<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IReadOnlyCollection<T> ?? enumerable.ToList();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as HashSet<T> ?? new HashSet<T>(enumerable);
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable != null && enumerable.Any();
        }
    }
}
