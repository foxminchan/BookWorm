using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class OrderEvaluator : IEvaluator
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

        if (
            specification.OrderExpressions.Count(x =>
                x.OrderType is OrderType.OrderBy or OrderType.OrderByDescending
            ) > 1
        )
        {
            throw new InvalidOperationException(
                "Only one OrderBy or OrderByDescending is allowed."
            );
        }

        var orderedQuery = specification.OrderExpressions.Aggregate<
            OrderExpression<T>?,
            IOrderedQueryable<T>?
        >(
            null,
            (queryable, orderExpression) =>
                orderExpression!.OrderType switch
                {
                    OrderType.OrderBy => queryable?.OrderBy(orderExpression.KeySelector),
                    OrderType.OrderByDescending => queryable?.OrderByDescending(
                        orderExpression.KeySelector
                    ),
                    OrderType.ThenBy => queryable?.ThenBy(orderExpression.KeySelector),
                    OrderType.ThenByDescending => queryable?.ThenByDescending(
                        orderExpression.KeySelector
                    ),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(OrderType),
                        "Invalid order type."
                    ),
                }
        );

        if (orderedQuery is not null)
        {
            query = orderedQuery;
        }

        return query;
    }
}
