using System;
using System.Linq.Expressions;
using Jalex.Infrastructure.Utils;

namespace Jalex.Repository.Utils
{
    internal class ReflectedTypeDescriptor<T> : ReflectedTypeDescriptorSimple
    {
        protected readonly Action<T, string> _idSetter;
        protected readonly Func<T, string> _idGetter;

        public Expression<Func<T, string>> IdGetterExpression { get; private set; }

        public ParameterExpression TypeParameter { get; private set; }
        public MemberExpression IdPropertyExpression { get; private set; }

        public ReflectedTypeDescriptor()
            : base(typeof(T))
        {
            IdGetterExpression = ExpressionProperties.GetPropertyGetterExpression<T, string>(IdPropertyName);
            _idGetter = IdGetterExpression.Compile();
            _idSetter = ExpressionProperties.GetPropertySetter<T, string>(IdPropertyName);

            TypeParameter = Expression.Parameter(typeof(T));
            IdPropertyExpression = Expression.Property(TypeParameter, IdPropertyName);
        }

        public string GetId(T target)
        {
            ParameterChecker.CheckForNull(target, "target");
            return _idGetter(target);
        }

        public void SetId(T target, string id)
        {
            _idSetter(target, id);
        }
    }
}
