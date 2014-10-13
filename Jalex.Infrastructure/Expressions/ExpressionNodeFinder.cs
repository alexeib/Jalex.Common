using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jalex.Infrastructure.Expressions
{
    public class ExpressionNodeFinder : ExpressionVisitor
    {
        private class ExpressionNodeFindingVisitor : ExpressionVisitor
        {
            private readonly List<Expression> _expressions;
            private readonly ExpressionType _targetType;

            public ExpressionNodeFindingVisitor(ExpressionType targetType)
            {
                _expressions = new List<Expression>();
                _targetType = targetType;
            }

            public IEnumerable<Expression> GetMatchingExpressions()
            {
                return _expressions;
            }

            #region Overrides of ExpressionVisitor

            public override Expression Visit(Expression node)
            {
                if (node != null && node.NodeType == _targetType)
                {
                    _expressions.Add(node);
                }

                return base.Visit(node);
            }

            #endregion
        }



        public IEnumerable<Expression> FindExpressionNodes(Expression root, ExpressionType type)
        {
            var finder = new ExpressionNodeFindingVisitor(type);
            finder.Visit(root);
            return finder.GetMatchingExpressions();
        }
    }
}