using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Cassandra
{
    internal class CassandraHelper
    {
        private readonly IReflectedTypeDescriptor _typeDescriptor;
        private IDictionary<string, IndexedAttribute> _clusteredIndices;
        private IDictionary<string, IndexedAttribute> _secondaryIndices;
        
        public CassandraHelper(IReflectedTypeDescriptor reflectedTypeDescriptor)
        {
            if (reflectedTypeDescriptor == null) throw new ArgumentNullException(nameof(reflectedTypeDescriptor));
            _typeDescriptor = reflectedTypeDescriptor;
            initIndices(_typeDescriptor.Properties);
        }

        public bool IsPropertyPartitionKey(string propName)
        {
            return _typeDescriptor.IdPropertyName == propName;
        }

        public bool IsPropertyClusteringKey(string propName)
        {
            return _clusteredIndices.ContainsKey(propName);
        }

        public bool IsPropertySecondaryIndex(string propName)
        {
            return _secondaryIndices.ContainsKey(propName);
        }

        public IndexedAttribute GetClusteringKeyAttribute(string propName)
        {
            IndexedAttribute attr;
            _clusteredIndices.TryGetValue(propName, out attr);
            return attr;
        }

        private void initIndices(IEnumerable<PropertyInfo> classProps)
        {
            var indexedPropAndAttrArr = (from prop in classProps
                                         from indexedAttribute in prop.GetCustomAttributes(true)
                                                                      .Where(a => a is IndexedAttribute)
                                                                      .Cast<IndexedAttribute>()
                                         orderby indexedAttribute.Index
                                         select new {PropName = prop.Name, Attribute = indexedAttribute}).ToArray();

            _clusteredIndices = indexedPropAndAttrArr
                .Where(c => c.Attribute.IndexType.HasFlag(IndexType.Clustered) && c.PropName != _typeDescriptor.IdPropertyName)
                .ToUniqueDictionary(x => x.PropName, x => x.Attribute);

            _secondaryIndices = indexedPropAndAttrArr
                .Where(c => c.Attribute.IndexType.HasFlag(IndexType.Secondary))
                .ToUniqueDictionary(x => x.PropName, x => x.Attribute);
        }
    }
}
