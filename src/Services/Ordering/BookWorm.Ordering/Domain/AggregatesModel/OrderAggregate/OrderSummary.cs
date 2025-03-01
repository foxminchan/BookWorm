using BookWorm.Contracts;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public sealed record OrderSummary(Guid Id, Status Status, decimal TotalPrice)
{
    public OrderSummary Apply(DeleteBasketCompleteCommand @event)
    {
        return this with { Status = Status.New };
    }

    public OrderSummary Apply(OrderCancelledEvent @event)
    {
        return this with { Status = Status.Cancelled };
    }

    public OrderSummary Apply(OrderCompletedEvent @event)
    {
        return this with { Status = Status.Completed };
    }
}
