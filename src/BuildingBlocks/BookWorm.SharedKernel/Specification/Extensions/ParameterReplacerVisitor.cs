using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Extensions;

public sealed class ParameterReplacerVisitor(
    ParameterExpression oldParameter,
    Expression newExpression
) : ExpressionVisitor
{
    private Expression _newExpression = newExpression;
    private ParameterExpression _oldParameter = oldParameter;

    internal void Update(ParameterExpression oldParameter, Expression newExpression)
    {
        (_oldParameter, _newExpression) = (oldParameter, newExpression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newExpression : node;
    }
}
