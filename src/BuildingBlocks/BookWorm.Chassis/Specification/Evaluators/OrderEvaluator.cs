using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Evaluators;

internal sealed class OrderEvaluator : IEvaluator
{
    private OrderEvaluator() { }

    public static OrderEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.OrderExpressions is null)
        {
            return query;
        }

        IOrderedQueryable<T>? orderedQuery = null;
        var hasPrimaryOrder = false;

        foreach (var orderExpression in specification.OrderExpressions)
        {
            if (IsPrimaryOrder(orderExpression.OrderType))
            {
                if (hasPrimaryOrder)
                {
                    throw new InvalidOperationException(
                        "Only one OrderBy or OrderByDescending is allowed."
                    );
                }

                hasPrimaryOrder = true;
            }

            orderedQuery = ApplyOrder(query, orderedQuery, orderExpression);
        }

        return orderedQuery ?? query;
    }

    private static bool IsPrimaryOrder(OrderType orderType)
    {
        return orderType is OrderType.OrderBy or OrderType.OrderByDescending;
    }

    private static IOrderedQueryable<T>? ApplyOrder<T>(
        IQueryable<T> query,
        IOrderedQueryable<T>? orderedQuery,
        OrderExpression<T> orderExpression
    )
        where T : class
    {
        return orderExpression.OrderType switch
        {
            OrderType.OrderBy => orderedQuery is null
                ? query.OrderBy(orderExpression.KeySelector)
                : orderedQuery.ThenBy(orderExpression.KeySelector),
            OrderType.OrderByDescending => orderedQuery is null
                ? query.OrderByDescending(orderExpression.KeySelector)
                : orderedQuery.ThenByDescending(orderExpression.KeySelector),
            OrderType.ThenBy => orderedQuery?.ThenBy(orderExpression.KeySelector),
            OrderType.ThenByDescending => orderedQuery?.ThenByDescending(
                orderExpression.KeySelector
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(orderExpression),
                orderExpression.OrderType,
                "Invalid order type."
            ),
        };
    }
}
