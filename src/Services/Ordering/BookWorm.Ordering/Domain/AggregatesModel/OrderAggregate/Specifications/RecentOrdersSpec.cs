using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public sealed class RecentOrdersSpec : Specification<Order>
{
    public RecentOrdersSpec(int count)
    {
        Query
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Take(count);
    }
}
