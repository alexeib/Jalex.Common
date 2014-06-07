using System;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public interface IReflectedTypeDescriptor<T> : IReflectedTypeDescriptor
    {
        Expression<Func<T, string>> IdGetterExpression { get; }
        ParameterExpression TypeParameter { get; }
        MemberExpression IdPropertyExpression { get; }

        string GetId(T target);
        void SetId(T target, string id);
    }
}
