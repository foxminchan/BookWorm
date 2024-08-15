using BookWorm.Ordering.Domain.OrderAggregate.Events;

namespace BookWorm.Ordering.Domain.OrderAggregate;

public record OrderState(Guid Id, Status Status)
{
    public OrderState Apply(OrderCreatedEvent @event)
    {
        return this with { Status = Status.Pending };
    }

    public OrderState Apply(OrderCompletedEvent @event)
    {
        return this with { Status = Status.Completed };
    }

    public OrderState Apply(OrderCancelledEvent @event)
    {
        return this with { Status = Status.Canceled };
    }
}
