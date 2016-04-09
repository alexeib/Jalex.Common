using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable != null && enumerable.Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [DebuggerStepThrough]
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            if (obj == null)
            {
                yield break;
            }
            yield return obj;
        }

        [DebuggerStepThrough]
        public static double AverageOrNaN(this IEnumerable<int> source)
        {
            if (source.IsNullOrEmpty()) return double.NaN;
            return source.Average();
        }

        [DebuggerStepThrough]
        public static double AverageOrNaN(this IEnumerable<double> source)
        {
            if (source.IsNullOrEmpty()) return double.NaN;
            return source.Average();
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
            return source;
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> items, Func<T, Task<bool>> predicate)
        {
            var itemTaskList = items.Select(item => new { Item = item, PredTask = predicate.Invoke(item) }).ToList();
            await Task.WhenAll(itemTaskList.Select(x => x.PredTask));
            return itemTaskList.Where(x => x.PredTask.Result).Select(x => x.Item);
        }
    }
}
