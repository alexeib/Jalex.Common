using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.ReflectedTypeDescriptor;

namespace Jalex.Infrastructure.Utils
{
    internal class ExpressionTypeChangingVisitor<TFrom, TTo> : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly Dictionary<string, PropertyInfo> _targetProperties;

        internal ExpressionTypeChangingVisitor(
            ParameterExpression parameter,
            IReflectedTypeDescriptorProvider reflectedTypeDescriptorProvider)
        {
            _parameter = parameter;
            var targetDescriptor = reflectedTypeDescriptorProvider.GetReflectedTypeDescriptor<TTo>();
            _targetProperties = targetDescriptor.Properties.ToDictionary(p => p.Name);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Type == typeof (TFrom)
                       ? _parameter
                       : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.ReflectedType != typeof(TFrom))
            {
                return node;
            }

            //only properties are allowed if you use fields then you need to extend
            // this method to handle them
            if (node.Member.MemberType != MemberTypes.Property)
                throw new NotImplementedException();

            //name of a member referenced in original expression in your 
            //sample Id in mine Prop
            var memberName = node.Member.Name;
            //find property on type T (=PersonData) by name

            if (!_targetProperties.ContainsKey(memberName))
            {
                throw new InvalidOperationException(string.Format("Target class {0} does not contain property {1}", typeof(TTo), memberName));
            }

            var otherMember = _targetProperties[memberName];
            //visit left side of this expression p.Id this would be p
            var inner = Visit(node.Expression);
            return Expression.Property(inner, otherMember);
        }
    }
}
