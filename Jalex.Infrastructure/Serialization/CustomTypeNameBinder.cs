using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Jalex.Infrastructure.Serialization
{
    public class CustomTypeNameBinder : SerializationBinder
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly ConcurrentDictionary<Type, string> _typeToNameDict = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<string, Type> _nameToTypeDict = new ConcurrentDictionary<string, Type>();
        // ReSharper restore StaticFieldInGenericType

        static CustomTypeNameBinder()
        {
            // warm cache
            foreach (var typeAndAttr in getTypesWithCustomTypeNameAttribute())
            {
                var type = typeAndAttr.Item1;
                var typeName = typeAndAttr.Item2.CustomTypeName;

                _typeToNameDict[type] = typeName;
                _nameToTypeDict[typeName] = type;
            }
        }

        #region Overrides of SerializationBinder

        public override Type BindToType(string assemblyName, string typeName)
        {
            var type = _nameToTypeDict.GetOrAdd(typeName, getTypeFromName);
            return type;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = _typeToNameDict.GetOrAdd(serializedType, getNameFromType);
        }

        #endregion

        private Type getTypeFromName(string name)
        {
            return _nameToTypeDict.GetOrAdd(name, getTypeEx);
        }

        private string getNameFromType(Type type)
        {
            return _typeToNameDict.GetOrAdd(type, t => t.FullName);
        }

        private static IEnumerable<Tuple<Type, CustomTypeNameAttribute>> getTypesWithCustomTypeNameAttribute()
        {
            var typesWithMyAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof (CustomTypeNameAttribute), false)
                where attributes != null && attributes.Length == 1
                select Tuple.Create(t, (CustomTypeNameAttribute)attributes.Single());
            return typesWithMyAttribute;
        }

        private static Type getTypeEx(string fullTypeName)
        {
            return Type.GetType(fullTypeName) ??
                   AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(fullTypeName))
                            .FirstOrDefault(t => t != null);
        }
    }
}
