using BookWorm.Contracts;

namespace BookWorm.Ordering.Domain.Projections;

public sealed class OrderSummaryInfo
{
    public Guid Id { get; set; }
    public Status Status { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class Projection : MultiStreamProjection<OrderSummaryInfo, Guid>
{
    public Projection()
    {
        Identity<DeleteBasketCompleteCommand>(e => e.Id);
        Identity<OrderCancelledEvent>(e => e.Order.Id);
        Identity<OrderCompletedEvent>(e => e.Order.Id);
    }

    public OrderSummaryInfo Apply(OrderSummaryInfo info, DeleteBasketCompleteCommand @event)
    {
        info.Status = Status.New;
        info.TotalPrice = @event.TotalMoney;
        return info;
    }

    public OrderSummaryInfo Apply(OrderSummaryInfo info, OrderCancelledEvent @event)
    {
        info.Status = Status.Cancelled;
        info.TotalPrice = @event.Order.TotalPrice;
        return info;
    }

    public OrderSummaryInfo Apply(OrderSummaryInfo info, OrderCompletedEvent @event)
    {
        info.Status = Status.Completed;
        info.TotalPrice = @event.Order.TotalPrice;
        return info;
    }
}
