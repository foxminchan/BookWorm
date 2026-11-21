using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public sealed class OrderFilterSpec : Specification<Order>
{
    public OrderFilterSpec(int pageIndex, int pageSize, Status? status = null, Guid? buyerId = null)
        : this(status, buyerId)
    {
        Query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ApplyPaging(pageIndex, pageSize);
    }

    public OrderFilterSpec(Status? status = null, Guid? buyerId = null)
    {
        Query.AsNoTracking();

        if (buyerId.HasValue)
        {
            var typedBuyerId = BuyerId.From(buyerId.Value);
            Query.Where(x => x.BuyerId == typedBuyerId);
        }

        if (status.HasValue)
        {
            Query.Where(x => x.Status == status.Value);
        }
    }

    public OrderFilterSpec(Guid orderId, Status orderStatus)
    {
        var typedOrderId = OrderId.From(orderId);
        Query.AsTracking().Where(x => x.Id == typedOrderId && x.Status == orderStatus);
    }

    public OrderFilterSpec(Guid orderId, Guid buyerId)
    {
        var typedOrderId = OrderId.From(orderId);
        var typedBuyerId = BuyerId.From(buyerId);
        Query.AsTracking().Where(x => x.Id == typedOrderId && x.BuyerId == typedBuyerId);
    }
}
