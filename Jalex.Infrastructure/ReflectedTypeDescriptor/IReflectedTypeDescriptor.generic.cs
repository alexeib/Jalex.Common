using System;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public interface IReflectedTypeDescriptor<T> : IReflectedTypeDescriptor
    {
        Expression<Func<T, Guid>> IdGetterExpression { get; }
        ParameterExpression TypeParameter { get; }
        MemberExpression IdPropertyExpression { get; }

        Guid GetId(T target);
        void SetId(T target, Guid id);

        object GetPropertyValue(string propertyName, T obj);
    }
}
