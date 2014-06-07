using System;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Extensions
{
    public static class ExpressionExtensions
    {
        #region Public Methods and Operators

        public static MemberInfo GetMemberInfo<T, TValue>(this Expression<Func<T, TValue>> property)
        {
            ParameterChecker.CheckForNull(property, "property");

            // current expression should be MemberExpression in general case
            Expression currentExpression = property.Body;

            // extract from unary expression
            UnaryExpression unaryExpression = currentExpression as UnaryExpression;
            if (unaryExpression != null)
            {
                currentExpression = unaryExpression.Operand;
            }

            // by this point the expression should be member expression
            MemberExpression memberExpression = currentExpression as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("MemberExpression cannot be acquired.", "property");
            }

            // return the acquired member info from the member expression
            return memberExpression.Member;
        }

        public static string GetParameterName<TRet>(this Expression<Func<TRet>> property)
        {
            // get its name
            return ((MemberExpression)property.Body).Member.Name;
        }

        public static string GetPropertyName<T, TRet>(this Expression<Func<T, TRet>> property)
        {
            // get member expression if it is inside unary expression
            Expression memberExpression = property.Body is UnaryExpression ? ((UnaryExpression)property.Body).Operand : property.Body;

            // get its name
            return ((MemberExpression)memberExpression).Member.Name;
        }

        #endregion
    }
}
