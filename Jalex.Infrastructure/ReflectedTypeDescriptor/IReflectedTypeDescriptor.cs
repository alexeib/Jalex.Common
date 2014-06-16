﻿using System.Reflection;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public interface IReflectedTypeDescriptor
    {
        string TypeName { get; }
        bool IsIdAutoGenerated { get; }
        string IdPropertyName { get; }
        PropertyInfo[] Properties { get; }
    }
}