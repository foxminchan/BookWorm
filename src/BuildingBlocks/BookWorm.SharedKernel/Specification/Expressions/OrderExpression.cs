using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class OrderExpression<T>
    where T : class
{
    private readonly Lazy<Func<T, object?>> _orderFunc;

    public OrderExpression(Expression<Func<T, object?>> keySelector, OrderType orderType)
    {
        _ = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

        KeySelector = keySelector;
        OrderType = orderType;

        _orderFunc = new(keySelector.Compile);
    }

    public OrderType OrderType { get; }

    public Expression<Func<T, object?>> KeySelector { get; }

    public Func<T, object?> OrderFunc => _orderFunc.Value;
}
