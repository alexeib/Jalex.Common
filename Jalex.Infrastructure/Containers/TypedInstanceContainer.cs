﻿using System;
using System.Collections;
using System.Collections.Generic;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Serialization;
using Magnum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jalex.Infrastructure.Containers
{
    public class TypedInstanceContainer<TKey, TInstance> : IEnumerable<TInstance>
        where TKey : IEquatable<TKey>
        where TInstance : class
    {
        private readonly Func<TInstance, TKey> _getKey;
        private readonly TKey _defaultKey;
        private readonly Dictionary<Tuple<Type, TKey>, TInstance> _instanceDictionary;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter>
                                                {
                                                    new StringEnumConverter()
                                                },
            TypeNameHandling = TypeNameHandling.Objects,
            Binder = new CustomTypeNameBinder()
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getKey">Function that can provide a unique object key. This function should never return null</param>
        /// <param name="defaultKey">The default key to use if one was not provided. This should not be null</param>
        public TypedInstanceContainer(Func<TInstance, TKey> getKey, TKey defaultKey)
            : this(getKey, defaultKey, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getKey">Function that can provide a unique object key. This function should never return null</param>
        /// <param name="defaultKey">The default key to use if one was not provided. This should not be null</param>
        /// <param name="serializedState">String representation of data to initialize the container. Null or empty string means empty container.</param>
        public TypedInstanceContainer(Func<TInstance, TKey> getKey, TKey defaultKey, string serializedState)
        {
            if (getKey == null) throw new ArgumentNullException(nameof(getKey));
            if (defaultKey == null) throw new ArgumentNullException(nameof(defaultKey));

            _getKey = getKey;
            _defaultKey = defaultKey;

            _instanceDictionary = string.IsNullOrEmpty(serializedState)
                ? new Dictionary<Tuple<Type, TKey>, TInstance>()
                : deserializeState(serializedState, _getKey);
        }

        /// <summary>
        /// Retrieves the instance with the default key
        /// </summary>
        /// <typeparam name="T">The type of instance to retrieve</typeparam>
        /// <returns>The instance or null if one does not exist</returns>
        public T GetDefault<T>() where T : class, TInstance
        {
            return Get<T>(_defaultKey);
        }

        /// <summary>
        /// Gets an instance with a specified key
        /// </summary>
        /// <typeparam name="T">The type of instance to get</typeparam>
        /// <param name="key">The instance key</param>
        /// <returns>The instance with the specified key or null</returns>
        public T Get<T>(TKey key) where T : class, TInstance
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _instanceDictionary.GetValueOrDefault(new Tuple<Type, TKey>(typeof(T), key)) as T;
        }

        /// <summary>
        /// Adds or replaces an instance
        /// </summary>
        /// <param name="instance">The instance to add or replace</param>
        public void Set(TInstance instance)
        {
            Guard.AgainstNull(instance, "instance");

            var instanceType = instance.GetType();
            var key = _getKey(instance);

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (key == null)
            {
                throw new InvalidOperationException("instance key cannot be null");
            }

            _instanceDictionary[new Tuple<Type, TKey>(instanceType, key)] = instance;
        }

        /// <summary>
        /// Adds or replaces an instance
        /// </summary>
        /// <param name="instance">The instance to add or replace</param>
        public void SetDefault(TInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var instanceType = instance.GetType();
            var key = _defaultKey;

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (key == null)
            {
                throw new InvalidOperationException("instance key cannot be null");
            }

            _instanceDictionary[new Tuple<Type, TKey>(instanceType, key)] = instance;
        }

        /// <summary>
        /// Adds removes the instance with default key
        /// </summary>
        /// <typeparam name="T">The type of instance to remove</typeparam>
        /// <returns>Whether anything was removed</returns>
        public bool RemoveDefault<T>() where T : class, TInstance
        {
            return Remove<T>(_defaultKey);
        }

        /// <summary>
        /// Removes an instance of specified type and with the specified key
        /// </summary>
        /// <typeparam name="T">The type of instance to remove</typeparam>
        /// <param name="key">The key of the instance to remove</param>
        /// <returns>Whether anything was removed</returns>
        public bool Remove<T>(TKey key) where T : class, TInstance
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            bool success = _instanceDictionary.Remove(new Tuple<Type, TKey>(typeof(T), key));
            return success;
        }

        /// <summary>
        /// Checks whether the container contains a default instance of the specified type
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns>Whether default instance exists in the container</returns>
        public bool ContainsDefault<T>() where T : class, TInstance
        {
            return Contains<T>(_defaultKey);
        }

        /// <summary>
        /// Checks whether the container contains an instance of the specified type with the specified key
        /// </summary>
        /// <typeparam name="T">The type of instance</typeparam>
        /// <param name="key">The instance key</param>
        /// <returns>Whether the instance exists in the container</returns>
        public bool Contains<T>(TKey key) where T : class, TInstance
        {
            return _instanceDictionary.ContainsKey(new Tuple<Type, TKey>(typeof(T), key));
        }

        public string SerializeToString()
        {
            var instances = _instanceDictionary.Values.ToCollection();
            var str = JsonConvert.SerializeObject(instances, _serializerSettings);
            return str;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TInstance> GetEnumerator()
        {
            return _instanceDictionary.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private static Dictionary<Tuple<Type, TKey>, TInstance> deserializeState(string serializedState, Func<TInstance, TKey> getKey)
        {
            var instances = JsonConvert.DeserializeObject<List<TInstance>>(serializedState, _serializerSettings);
            var dict = new Dictionary<Tuple<Type, TKey>, TInstance>(instances.Count);
            foreach (var instance in instances)
            {
                var key = new Tuple<Type, TKey>(instance.GetType(), getKey(instance));
                dict[key] = instance;
            }
            return dict;
        }
    }
}
