using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public sealed class OrderFilterSpec : Specification<Order>
{
    public OrderFilterSpec(int pageIndex, int pageSize, Status? status = null, Guid? buyerId = null)
        : this(status, buyerId)
    {
        Query.OrderByDescending(x => x.CreatedAt).ApplyPaging(pageIndex, pageSize);
    }

    public OrderFilterSpec(Status? status = null, Guid? buyerId = null)
    {
        Query.AsNoTracking();

        if (buyerId.HasValue)
        {
            Query.Where(x => x.BuyerId == buyerId.Value);
        }

        if (status.HasValue)
        {
            Query.Where(x => x.Status == status.Value);
        }
    }

    public OrderFilterSpec(Guid orderId, Status orderStatus)
    {
        Query.AsTracking().Where(x => x.Id == orderId && x.Status == orderStatus);
    }

    public OrderFilterSpec(Guid orderId, Guid buyerId)
    {
        Query.AsTracking().Where(x => x.Id == orderId && x.BuyerId == buyerId);
    }
}
