using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EmitMapper.MappingConfiguration;
using EmitMapper.MappingConfiguration.MappingOperations;
using EmitMapper.Utils;

namespace Jalex.Infrastructure.Mapping
{
    public class ExtendedEmitMapperConfig<TSrc, TDst> : DefaultMapConfig
    {
        private readonly Dictionary<string, Func<TSrc, object>> _properties = new Dictionary<string, Func<TSrc, object>>();

        public ExtendedEmitMapperConfig<TSrc, TDst> ForMember(string property, Func<TSrc, object> func)
        {
            if (!_properties.ContainsKey(property))
                _properties.Add(property, func);
            return this;
        }

        public ExtendedEmitMapperConfig<TSrc, TDst> ForMember(Expression<Func<TDst, object>> dstMember, Func<TSrc, object> func)
        {
            var prop = ReflectionHelper.FindProperty(dstMember);
            return ForMember(prop.Name, func);
        }

        public ExtendedEmitMapperConfig<TSrc, TDst> Ignore(Expression<Func<TDst, object>> dstMember)
        {
            var prop = ReflectionHelper.FindProperty(dstMember);
            IgnoreMembers<TSrc, TDst>(new[] { prop.Name });
            return this;
        }

        public override IMappingOperation[] GetMappingOperations(Type from, Type to)
        {
            var list = new List<IMappingOperation>();
            list.AddRange(base.GetMappingOperations(from, to));
            list.AddRange(
                    FilterOperations(
                        from,
                        to,
                        ReflectionUtils.GetPublicFieldsAndProperties(to)
                        .Where(f => _properties.ContainsKey(f.Name))
                        .Select(
                            m =>
                            (IMappingOperation)new DestWriteOperation
                            {
                                Destination = new MemberDescriptor(m),
                                Getter =
                                    (ValueGetter<object>)
                                    (
                                        (value, state) => ValueToWrite<object>.ReturnValue(_properties[m.Name]((TSrc)value)))
                            }
                        )
                    )
                );

            return list.ToArray();
        }
    }

    class ReflectionHelper
    {
        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expression = lambdaExpression;
            bool flag = false;
            while (!flag)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expression = ((LambdaExpression)expression).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expression;
                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter && memberExpression.Expression.NodeType != ExpressionType.Convert)
                            throw new ArgumentException(string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression), "lambdaExpression");
                        return memberExpression.Member;
                    default:
                        flag = true;
                        break;
                }
            }
            return null;
        }

        public static object GetValue(string property, object obj)
        {
            PropertyInfo pi = obj.GetType().GetProperty(property);
            return pi.GetValue(obj, null);
        }
    }
}
