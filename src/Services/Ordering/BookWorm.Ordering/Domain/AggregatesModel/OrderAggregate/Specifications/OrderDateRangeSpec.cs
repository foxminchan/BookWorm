using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public sealed class OrderDateRangeSpec : Specification<Order>
{
    public OrderDateRangeSpec(DateTime start, DateTime end)
    {
        Query.AsNoTracking().Where(order => order.CreatedAt >= start && order.CreatedAt < end);
    }
}
