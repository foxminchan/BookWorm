using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public static class OrderSpecExpression
{
    public static void ApplyPaging(
        this ISpecificationBuilder<Order> builder,
        int pageIndex,
        int pageSize
    )
    {
        builder.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }
}
