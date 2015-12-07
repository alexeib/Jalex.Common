using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Jalex.Infrastructure.Serialization
{
    public class JsonMissingTypeObjectRemover
    {
        private struct TypeNameKey : IEquatable<TypeNameKey>
        {
            internal readonly string AssemblyName;
            internal readonly string TypeName;

            public TypeNameKey(string assemblyName, string typeName)
            {
                AssemblyName = assemblyName;
                TypeName = typeName;
            }

            public override int GetHashCode()
            {
                return (AssemblyName?.GetHashCode() ?? 0) ^ (TypeName?.GetHashCode() ?? 0);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeNameKey))
                    return false;

                return Equals((TypeNameKey)obj);
            }

            public bool Equals(TypeNameKey other)
            {
                return (AssemblyName == other.AssemblyName && TypeName == other.TypeName);
            }
        }

        private static readonly Regex _typeNameRegex = new Regex("\"\\$type\": ?\"(?<type>[^\"]+), (?<assembly>[^\"]+)\"", RegexOptions.Compiled);
        private static readonly ConcurrentDictionary<TypeNameKey, Type> _loadedTypes = new ConcurrentDictionary<TypeNameKey, Type>();

        public string RemoveMissingTypesFromJsonString(string jsonString)
        {
            if (jsonString == null) return null;

            var matchedTypes = _typeNameRegex.Matches(jsonString);
            var missingTypeIndicies = from Match match in matchedTypes
                                      let assembly = match.Groups["assembly"]
                                      let type = match.Groups["type"]
                                      where !typeExists(assembly.Value, type.Value)
                                      select match.Index;

            foreach (var idx in missingTypeIndicies.OrderByDescending(x => x))
            {
                int startIdx = idx;
                int numQuotes = 0;
                while (true)
                {
                    if (jsonString[startIdx] == '"') numQuotes++;
                    if (numQuotes == 3 && (jsonString[startIdx - 1] == ',' || jsonString[startIdx - 1] == '{'))
                    {
                        break;
                    }
                    startIdx--;
                }

                int endIdx = idx;
                int numBrackets = 1;
                while (numBrackets != 0)
                {
                    if (jsonString[endIdx] == '{') numBrackets++;
                    if (jsonString[endIdx] == '}') numBrackets--;
                    endIdx++;
                }

                if (jsonString[endIdx] == ',')
                {
                    endIdx++;
                }

                jsonString = jsonString.Remove(startIdx, endIdx - startIdx);
            }

            return jsonString;
        }

        private bool typeExists(string assemblyName, string typeName)
        {
            return _loadedTypes.GetOrAdd(new TypeNameKey(assemblyName, typeName), loadType) != null;
        }

        private Type loadType(TypeNameKey key)
        {
            var assemblyName = key.AssemblyName;
            var typeName = key.TypeName;

            var assembly = Assembly.LoadWithPartialName(assemblyName);

            if (assembly == null)
            {
                // will find assemblies loaded with Assembly.LoadFile outside of the main directory
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly a in loadedAssemblies)
                {
                    if (a.FullName == assemblyName)
                    {
                        assembly = a;
                        break;
                    }
                }
            }

            Type type = assembly?.GetType(typeName);
            return type;
        }
    }
}