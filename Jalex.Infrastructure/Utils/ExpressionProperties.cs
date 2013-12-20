using System;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.Utils
{
    public static class ExpressionProperties
    {
        // returns property getter
        public static Func<TObject, TProperty> GetPropertyGetter<TObject, TProperty>(string propertyName)
        {
            Func<TObject, TProperty> result = GetPropertyGetterExpression<TObject, TProperty>(propertyName).Compile();

            return result;
        }

        public static Expression<Func<TObject, TProperty>> GetPropertyGetterExpression<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");

            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);

            return Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression);
        }

        // returns property setter:
        public static Action<TObject, TProperty> GetPropertySetter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));

            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);

            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
            (
                Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();

            return result;
        }
    }
}
