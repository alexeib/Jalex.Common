using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Extensions;

namespace Jalex.TestUtils.xUnit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class InlineConstructorDataAttribute : DataAttribute
    {
        private readonly Type _type;

        public InlineConstructorDataAttribute(Type constructedType)
        {
            _type = constructedType;
        }

        public Type ConstructedType
        {
            get { return _type; }
        }

        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest,
             Type[] parameterTypes)
        {
            if (parameterTypes.Length != 1)
                yield break;

            ConstructorInfo info = _type.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                 null, new Type[] { }, null);
            if (info == null)
                yield break;
            yield return new object[] { info.Invoke(new object[] { }) };
        }
    }
}
