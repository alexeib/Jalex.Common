using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jalex.Infrastructure.Caching;
using Jalex.Infrastructure.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Utils;

namespace Jalex.Services.Caching
{
    public class IndexCache<T> : IIndexCache<T>
    {
        private readonly SortedSet<string> _indexedProperties;
        private readonly ICache<string, string> _cache;
        private readonly IReflectedTypeDescriptor<T> _typeDescriptor;

        public IndexCache(
            IEnumerable<string> indexedProperties,
            ICacheFactory cacheFactory,
            Action<ICacheStrategyConfiguration> cacheConfiguration,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ParameterChecker.CheckForEmptyCollection(indexedProperties, "indexedProperties");
            ParameterChecker.CheckForNull(cacheFactory, "cacheFactory");
            ParameterChecker.CheckForNull(cacheConfiguration, "cacheConfiguration");
            ParameterChecker.CheckForNull(typeDescriptorProvider, "typeDescriptorProvider");

            _cache = cacheFactory.Create<string, string>(cacheConfiguration);
            // ReSharper disable once PossibleMultipleEnumeration
            _indexedProperties = new SortedSet<string>(indexedProperties);
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
        }

        public IEnumerable<string> IndexedProperties { get { return _indexedProperties; } }

        public void Index(T obj)
        {
            ParameterChecker.CheckForNull(obj, "obj");

            var indexKey = getIndexKey(obj);
            var objId = _typeDescriptor.GetId(obj);

            _cache.Set(indexKey, objId);
        }

        public void DeIndex(T obj)
        {
            ParameterChecker.CheckForNull(obj, "obj");

            var indexKey = getIndexKey(obj);

            _cache.DeleteById(indexKey);
        }

        public void DeIndexByQuery(Expression<Func<T, bool>> query)
        {
            ParameterChecker.CheckForNull(query, "query");

            var indexKey = getIndexKeyFromQuery(query); 

            if (indexKey != null)
            {
                _cache.DeleteById(indexKey);
            }
        }

        public string FindIdByQuery(Expression<Func<T, bool>> query)
        {
            ParameterChecker.CheckForNull(query, "query");

            var indexKey = getIndexKeyFromQuery(query); 
            return indexKey != null ? _cache.GetOrDefault(indexKey) : null;
        }        

        private IDictionary<string, object> getEqualityCheckedProps(Expression<Func<T, bool>> query)
        {
            Dictionary<string, object> equalityCheckedProps = new Dictionary<string, object>();

            var nodeFinder = new ExpressionNodeFinder();
            var matchingExpressions = nodeFinder.FindExpressionNodes(query, ExpressionType.Equal).OfType<BinaryExpression>();
            foreach (var expr in matchingExpressions)
            {
                MemberExpression propExpr;
                Expression otherExpr;

                var leftAsMember = expr.Left as MemberExpression;
                var rightAsMember = expr.Right as MemberExpression;

                if (leftAsMember != null && leftAsMember.Member.ReflectedType == typeof(T))
                {
                    propExpr = leftAsMember;
                    otherExpr = expr.Right;
                }
                else if (rightAsMember != null && rightAsMember.Member.ReflectedType == typeof(T))
                {
                    propExpr = rightAsMember;
                    otherExpr = expr.Left;
                }
                else
                {
                    continue;
                }

                string propName = propExpr.Member.Name;
                object val = ExpressionUtils.GetExpressionValue<object>(otherExpr);

                equalityCheckedProps[propName] = val;
            }

            return equalityCheckedProps;
        }

        private string getIndexKey(T obj)
        {
            List<object> propValueList =
                _indexedProperties
                .Select(indexedProp => _typeDescriptor.GetPropertyValue(indexedProp, obj))
                .ToList();

            var serializedPropValues = propValueList.ToJson();
            return serializedPropValues;
        }

        private string getIndexKeyFromQuery(Expression<Func<T, bool>> query)
        {
            var equalityCheckedProps = getEqualityCheckedProps(query);
            string serializedPropValues = null;
            if (_indexedProperties.All(equalityCheckedProps.ContainsKey))
            {
                List<object> propValueList =
                    _indexedProperties
                        .Select(indexedProp => equalityCheckedProps[indexedProp])
                        .ToList();
                serializedPropValues = propValueList.ToJson();
            }
            return serializedPropValues;
        }
    }
}
