using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;

namespace Jalex.Repository.Cassandra
{
    internal class CassandraHelper
    {
        private readonly IReflectedTypeDescriptor _typeDescriptor;
        private string _partitionKey;
        private Dictionary<string, IndexedAttribute> _clusteredIndices;
        private Dictionary<string, IndexedAttribute> _secondaryIndices;
        
        public bool HasClusteredIndices { get; private set; }

        public CassandraHelper(Type entityType)
        {
            Guard.AgainstNull(entityType, "entityType");

            IReflectedTypeDescriptorProvider provider = new ReflectedTypeDescriptorProvider();
            _typeDescriptor = provider.GetReflectedTypeDescriptor(entityType);
            initClusteredIndices(_typeDescriptor.Properties);
        }

        public bool IsPropertyPartitionKey(string propName)
        {
            if (HasClusteredIndices)
            {
                return _partitionKey == propName;
            }
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

        private void initClusteredIndices(IEnumerable<PropertyInfo> classProps)
        {
            var indexedPropAndAttrArr = (from prop in classProps
                                     let indexedAttribute = (IndexedAttribute) prop.GetCustomAttributes(true).FirstOrDefault(a => a is IndexedAttribute)
                                     where indexedAttribute != null
                                     orderby indexedAttribute.Index
                                     select new {PropName = prop.Name, Attribute = indexedAttribute}).ToArray();

            HasClusteredIndices = indexedPropAndAttrArr.Any(c => c.Attribute.IsClustered);

            if (HasClusteredIndices)
            {
                _partitionKey = _typeDescriptor.IdPropertyName;
            }

            _clusteredIndices = indexedPropAndAttrArr
                .Where(c => c.Attribute.IsClustered && c.PropName != _partitionKey)
                .ToDictionary(x => x.PropName, x => x.Attribute);

            _secondaryIndices = indexedPropAndAttrArr
                .Where(c => !c.Attribute.IsClustered)
                .ToDictionary(x => x.PropName, x => x.Attribute);
        }
    }
}
