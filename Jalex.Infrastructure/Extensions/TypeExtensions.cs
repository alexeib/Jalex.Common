using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jalex.Infrastructure.Extensions
{
    public static class TypeExtensions
    {
        public static Type GetNullableUnderlyingType(this Type t)
        {
            if (t != null && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) return Nullable.GetUnderlyingType(t);
            else return t;
        }

        public static bool IsNullable(this Type t)
        {
            return ((!t.IsValueType || // if it is reference type or it is nullable
                     (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))));
        }

        private static readonly Type[] _numericTypes = {
                                                           typeof (Byte), typeof (SByte), typeof (Int16), typeof (UInt16), typeof (Int32), typeof (UInt32), typeof (Int64),
                                                           typeof (UInt64), typeof (Single), typeof (Double), typeof (Decimal)
                                                       };

        public static bool IsNumericType(this Type t)
        {
            t = GetNullableUnderlyingType(t);
            return _numericTypes.Contains(t);
        }

        public static bool IsDerivedFrom(this Type typeToCheck, Type targetType)
        {
            if (typeToCheck == targetType) return true;
            if (typeToCheck.BaseType == null) return false;
            return IsDerivedFrom(typeToCheck.BaseType, targetType);
        }

        public static bool IsAnonymousType(this Type type)
        {
            if (type.Name.StartsWith("<>") && !type.Name.Contains("AnonymousType") && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0)
            {
                return true;
            }

            return false;
        }
    }
}
