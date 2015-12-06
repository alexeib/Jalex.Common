using System;

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
            if (customTypeName == null) throw new ArgumentNullException(nameof(customTypeName));
            CustomTypeName = customTypeName;
        }
    }
}
