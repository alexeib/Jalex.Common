using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jalex.Infrastructure.Containers
{
    public class TypedInstanceContainer<TInstance> : IEnumerable<TInstance>
        where TInstance : class
    {
        private readonly Dictionary<Type, ICollection<TInstance>> _instancesDictionary;

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
        public TypedInstanceContainer()
            : this(null)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializedState">String representation of data to initialize the container. Null or empty string means empty container.</param>
        public TypedInstanceContainer(string serializedState)
        {
            _instancesDictionary = string.IsNullOrEmpty(serializedState)
                ? new Dictionary<Type, ICollection<TInstance>>()
                : deserializeState(serializedState);
        }

        /// <summary>
        /// Retrieves the instance of a class. Throws an exception if there are multiple instances of this class stored
        /// </summary>
        /// <typeparam name="T">The type of instance to retrieve</typeparam>
        /// <returns>The instance or null if one does not exist</returns>
        public T GetSingle<T>() where T : class, TInstance
        {
            return GetAll<T>()
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets all instances of a class
        /// </summary>
        /// <typeparam name="T">The type of instance to get</typeparam>
        public IEnumerable<T> GetAll<T>() where T : class, TInstance
        {
            ICollection<TInstance> collection;
            if (_instancesDictionary.TryGetValue(typeof(T), out collection))
            {
                return collection.Cast<T>()
                                 .ToCollection();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Adds or replaces an instance
        /// </summary>
        /// <param name="instance">The instance to add or replace</param>
        public void Add(TInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            addToDict(_instancesDictionary, instance);
        }

        /// <summary>
        /// Removes an instance of an item if it exists in this container
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>Whether anything was removed</returns>
        public bool Remove(TInstance item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var type = item.GetType();
            ICollection<TInstance> collection;
            if (_instancesDictionary.TryGetValue(type, out collection))
            {
                // ReSharper disable once PossibleNullReferenceException
                return collection.Remove(item);
            }
            return false;
        }

        /// <summary>
        /// Checks whether the container contains any instances of the specified type
        /// </summary>
        /// <typeparam name="T">The type of instance</typeparam>
        /// <returns>Whether any instances exist in the container</returns>
        public bool ContainsAny<T>() where T : class, TInstance
        {
            return GetAll<T>().Any();
        }

        public string SerializeToString()
        {
            var instances = this.ToCollection();
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
            return _instancesDictionary.Values.SelectMany(v => v).GetEnumerator();
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

        private static Dictionary<Type, ICollection<TInstance>> deserializeState(string serializedState)
        {
            var instances = JsonConvert.DeserializeObject<List<TInstance>>(serializedState, _serializerSettings);
            var dict = new Dictionary<Type, ICollection<TInstance>>(instances.Count);
            foreach (var instance in instances)
            {
                addToDict(dict, instance);
            }
            return dict;
        }

        private static void addToDict(IDictionary<Type, ICollection<TInstance>> dict, TInstance item)
        {
            var instanceType = item.GetType();

            ICollection<TInstance> collection;
            if (!dict.TryGetValue(instanceType, out collection))
            {
                collection = new List<TInstance>();
                dict[instanceType] = collection;
            }

            collection.Add(item);
        }
    }
}
