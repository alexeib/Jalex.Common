﻿using System;
using System.Linq.Expressions;
using Jalex.Infrastructure.ReflectedTypeDescriptor;

namespace Jalex.Infrastructure.Utils
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
    }
}