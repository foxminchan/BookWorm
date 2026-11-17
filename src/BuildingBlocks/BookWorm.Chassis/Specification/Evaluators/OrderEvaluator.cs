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
            if (orderExpression.OrderType is OrderType.OrderBy or OrderType.OrderByDescending)
            {
                if (hasPrimaryOrder)
                {
                    throw new InvalidOperationException(
                        "Only one OrderBy or OrderByDescending is allowed."
                    );
                }

                hasPrimaryOrder = true;
            }

            orderedQuery = orderExpression.OrderType switch
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
                    nameof(OrderType),
                    "Invalid order type."
                ),
            };
        }

        return orderedQuery ?? query;
    }
}
