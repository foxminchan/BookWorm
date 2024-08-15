using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Events;
using Marten.Events.Projections;

namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderQuery(Guid Id, Status Status);

public sealed class OrderProjection : MultiStreamProjection<OrderQuery, Guid>
{
    public OrderProjection()
    {
        Identity<OrderCreatedEvent>(e => e.Id);
        Identity<OrderCompletedEvent>(e => e.Id);
        Identity<OrderCancelledEvent>(e => e.Id);
    }

    public OrderQuery Apply(OrderQuery query, OrderCreatedEvent @event)
    {
        return query with { Status = Status.Pending };
    }

    public OrderQuery Apply(OrderQuery query, OrderCompletedEvent @event)
    {
        return query with { Status = Status.Completed };
    }

    public OrderQuery Apply(OrderQuery query, OrderCancelledEvent @event)
    {
        return query with { Status = Status.Canceled };
    }
}
