using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Expressions
{
    public static class ExpressionUtils
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

            if (typeof (TProperty) != propertyGetterExpression.Type)
            {
                propertyGetterExpression = Expression.Convert(propertyGetterExpression, typeof (TProperty));
            }
            
            return Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression);
        }

        // returns property setter:
        public static Action<TObject, TProperty> GetPropertySetter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));

            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);

            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>(
                Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();
            
            return result;
        }

        public static Expression<Func<TTo, TRet>> ChangeType<TFrom, TTo, TRet>(
            Expression<Func<TFrom, TRet>> expression, 
            IReflectedTypeDescriptorProvider reflectedTypeDescriptorProvider)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TTo));
            var visitor = new ExpressionTypeChangingVisitor<TFrom, TTo>(parameter, reflectedTypeDescriptorProvider);
            Expression body = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<TTo, TRet>>(body, parameter);
        }

        public static object GetExpressionValue(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var ce = expression as ConstantExpression;
            if (ce != null)
            {
                return ce.Value;
            }

            var ma = expression as MemberExpression;
            if (ma != null)
            {
                var se = ma.Expression;
                object val = null;
                if (se != null)
                {
                    val = GetExpressionValue(se);
                }

                var fi = ma.Member as FieldInfo;
                if (fi != null)
                {
                    return fi.GetValue(val);
                }
                var pi = ma.Member as PropertyInfo;
                if (pi != null)
                {
                    return pi.GetValue(val);
                }
            }

            var mce = expression as MethodCallExpression;
            if (mce != null)
            {
                var obj = mce.Object != null ? GetExpressionValue(mce.Object) : null;
                return mce.Method.Invoke(obj, mce.Arguments.Select(GetExpressionValue).ToArray());
            }

            var le = expression as LambdaExpression;
            if (le != null)
            {
                if (le.Parameters.Count == 0)
                {
                    return GetExpressionValue(le.Body);
                }
                return le.Compile().DynamicInvoke();
            }

            var dynamicInvoke = Expression.Lambda(expression).Compile().DynamicInvoke();
            return dynamicInvoke;
        }

        public static Expression<Func<T, bool>> GetExpressionForPropertyEquality<T, TVal>(TVal val, MemberExpression memberRetrievalExpression)
        {
            return GetExpressionForPropertyEquality<T, TVal>(val, memberRetrievalExpression, Expression.Parameter(typeof (T)));
;        }

        public static Expression<Func<T, bool>> GetExpressionForPropertyEquality<T, TVal>(TVal val, MemberExpression memberRetrievalExpression, ParameterExpression paramExpr)
        {
            var idValueExpr = Expression.Constant(val);
            var idEqualsValueExpr = Expression.Equal(memberRetrievalExpression, idValueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(idEqualsValueExpr, paramExpr);
            return lambda;
        }
    }
}
