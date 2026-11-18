namespace BookWorm.Ordering.Infrastructure.EventStore.Projections;

internal sealed record OrderSummary(Guid Id, Status Status, decimal TotalPrice)
{
    public static OrderSummary Create(OrderPlacedEvent @event)
    {
        return new((Guid)@event.Order.Id, Status.New, @event.Order.TotalPrice);
    }

    public OrderSummary Apply(OrderCancelledEvent @event)
    {
        return this with { Status = Status.Cancelled, TotalPrice = @event.Order.TotalPrice };
    }

    public OrderSummary Apply(OrderCompletedEvent @event)
    {
        return this with { Status = Status.Completed, TotalPrice = @event.Order.TotalPrice };
    }

    public static OrderSummary? Transition(OrderSummary? current, DomainEvent @event)
    {
        return @event switch
        {
            OrderPlacedEvent placed => Create(placed),
            OrderCancelledEvent cancelled => current?.Apply(cancelled),
            OrderCompletedEvent completed => current?.Apply(completed),
            _ => current,
        };
    }
}
