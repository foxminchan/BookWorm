using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Extensions;

public sealed class ParameterReplacerVisitor(
    ParameterExpression oldParameter,
    Expression newExpression
) : ExpressionVisitor
{
    private ParameterExpression _oldParameter = oldParameter;
    private Expression _newExpression = newExpression;

    internal void Update(ParameterExpression oldParameter, Expression newExpression) =>
        (_oldParameter, _newExpression) = (oldParameter, newExpression);

    protected override Expression VisitParameter(ParameterExpression node) =>
        node == _oldParameter ? _newExpression : node;
}
