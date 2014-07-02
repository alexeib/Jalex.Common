using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Containers
{
    public class TypeInstanceContainer<T> : IEnumerable<T> where T : class
    {
        private readonly ConcurrentDictionary<Type, T> _metricDictionary;

        public TypeInstanceContainer()
        {
            _metricDictionary = new ConcurrentDictionary<Type, T>();
        }

        public TMetric Get<TMetric>() where TMetric : class, T
        {
            var metricType = typeof(TMetric);
            T ret;
            _metricDictionary.TryGetValue(metricType, out ret);
            return (TMetric)ret;

        }

        public void Set(T metric)
        {
            ParameterChecker.CheckForVoid(() => metric);

            var metricType = metric.GetType();
            _metricDictionary[metricType] = metric;
        }

        public bool Remove<TMetric>() where TMetric : class, T
        {
            var metricType = typeof(TMetric);
            T ret;
            bool success = _metricDictionary.TryRemove(metricType, out ret);
            return success;
        }

        public bool Contains<TMetric>() where TMetric : class, T
        {
            var metricType = typeof(TMetric);
            var contains = _metricDictionary.ContainsKey(metricType);
            return contains;
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = _metricDictionary.Values.GetEnumerator();
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
