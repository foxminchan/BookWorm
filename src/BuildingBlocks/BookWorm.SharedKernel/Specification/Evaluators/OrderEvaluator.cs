using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class OrderEvaluator : IEvaluator
{
    private OrderEvaluator() { }

    public static OrderEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.OrderBy is null)
        {
            return query;
        }

        if (
            specification.OrderBy.Count(x =>
                x.OrderType is OrderType.OrderBy or OrderType.OrderByDescending
            ) > 1
        )
        {
            throw new InvalidOperationException(
                "Only one OrderBy or OrderByDescending is allowed."
            );
        }

        IOrderedQueryable<T>? orderedQuery = specification.OrderBy.Aggregate<
            OrderExpression<T>?,
            IOrderedQueryable<T>?
        >(
            null,
            (current, orderExpression) =>
                orderExpression!.OrderType switch
                {
                    OrderType.OrderBy => current!.OrderBy(orderExpression.KeySelector),
                    OrderType.OrderByDescending => current!.OrderByDescending(
                        orderExpression.KeySelector
                    ),
                    OrderType.ThenBy => current!.ThenBy(orderExpression.KeySelector),
                    OrderType.ThenByDescending => current!.ThenByDescending(
                        orderExpression.KeySelector
                    ),
                    _ => throw new ArgumentOutOfRangeException(nameof(OrderType)),
                }
        );

        if (orderedQuery is not null)
        {
            query = orderedQuery;
        }

        return query;
    }
}
