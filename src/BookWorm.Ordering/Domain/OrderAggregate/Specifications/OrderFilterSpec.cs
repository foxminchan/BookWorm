using Ardalis.Specification;

namespace BookWorm.Ordering.Domain.OrderAggregate.Specifications;

public sealed class OrderFilterSpec : Specification<Order>
{
    public OrderFilterSpec(Guid orderId)
    {
        Query.Where(o => o.Id == orderId);
    }

    public OrderFilterSpec(Guid buyerId, Guid orderId)
    {
        Query.Where(o => o.BuyerId == buyerId && o.Id == orderId);
    }
}
