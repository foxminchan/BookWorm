using Ardalis.Specification;

namespace BookWorm.Ordering.Domain.BuyerAggregate.Specifications;

public sealed class BuyerFilterSpec : Specification<Buyer>
{
    public BuyerFilterSpec(Guid buyerId, Guid orderId)
    {
        Query.Where(b => b.Id == buyerId && b.Orders.Any(o => o.Id == orderId));
    }
}
