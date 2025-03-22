using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class OrderExpression<T>
    where T : class
{
    public OrderExpression(Expression<Func<T, object?>> keySelector, OrderType orderType)
    {
        _ = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

        KeySelector = keySelector;
        OrderType = orderType;
    }

    public Expression<Func<T, object?>> KeySelector { get; }

    public OrderType OrderType { get; }
}
