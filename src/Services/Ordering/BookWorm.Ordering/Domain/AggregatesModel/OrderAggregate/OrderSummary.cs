namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public sealed record OrderSummary(Guid Id, Status Status, decimal TotalPrice)
{
    public static OrderSummary Create(OrderPlacedEvent @event)
    {
        return new(@event.Order.Id, Status.New, @event.Order.TotalPrice);
    }

    public static OrderSummary Apply(OrderCancelledEvent @event)
    {
        return new(@event.Order.Id, Status.Cancelled, @event.Order.TotalPrice);
    }

    public static OrderSummary Apply(OrderCompletedEvent @event)
    {
        return new(@event.Order.Id, Status.Completed, @event.Order.TotalPrice);
    }
}
