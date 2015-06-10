using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jalex.Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static T[] ToArrayEfficient<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as T[] ?? enumerable.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static IReadOnlyCollection<T> ToCollection<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IReadOnlyCollection<T> ?? enumerable.ToList();
        }

        [DebuggerStepThrough]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as HashSet<T> ?? new HashSet<T>(enumerable);
        }

        public static IDictionary<TK, TSource> ToUniqueDictionary<TK, TSource>(this IEnumerable<TSource> enumerable, Func<TSource, TK> keySelector)
        {
            return enumerable.ToUniqueDictionary(keySelector, x => x);
        }

        public static IDictionary<TK, TV> ToUniqueDictionary<TK, TV, TSource>(this IEnumerable<TSource> enumerable, Func<TSource, TK> keySelector, Func<TSource, TV> valueSelector)
        {
            var lookup = enumerable.ToLookup(keySelector, valueSelector);
            return lookup.ToDictionary(items => items.Key, items => items.First());
        }

        [DebuggerStepThrough]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable != null && enumerable.Any();
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !IsNotNullOrEmpty(enumerable);
        }

        public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator)
        {
            if (enumerable == null)
            {
                return null;
            }

            return string.Join(separator, enumerable);
        }
    }
}
