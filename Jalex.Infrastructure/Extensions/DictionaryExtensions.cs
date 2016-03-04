using System;
using System.Collections.Generic;
using System.Linq;

namespace Jalex.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetValueOrDefault(key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.GetValueOrDefault(key, () => defaultValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueFunc)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValueFunc == null ? default(TValue) : defaultValueFunc();
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> other)
        {
            return source.Concat(other)
                         .ToUniqueDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
