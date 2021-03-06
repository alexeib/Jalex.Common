﻿using System.Collections.Generic;
using System.Reflection;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public interface IReflectedTypeDescriptor
    {
        string TypeName { get; }
        bool IsIdAutoGenerated { get; }
        string IdPropertyName { get; }
        IEnumerable<PropertyInfo> Properties { get; }
        bool HasClusteredIndices { get; }
    }
}
