using System;
using System.Linq;

namespace Jalex.Infrastructure.Utils
{
    public static class TypeUtils
    {
        public static Type GetTypeFromLoadedAssemblies(string fullTypeName)
        {
            return Type.GetType(fullTypeName) ??
                   AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(fullTypeName))
                            .FirstOrDefault(t => t != null);
        }
    }
}
