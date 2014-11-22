using System;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Serialization
{
    /// <summary>
    /// Instructs serializers using CustomTypeNameBinder to serialize the type name as a custom string
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class CustomTypeNameAttribute : Attribute
    {
        public string CustomTypeName { get; private set; }

        public CustomTypeNameAttribute(string customTypeName)
        {
            Guard.AgainstNull(customTypeName, "customTypeName");
            CustomTypeName = customTypeName;
        }
    }
}
