using BookWorm.Contracts;
using Marten.Schema;

namespace BookWorm.Ordering.Domain.Projections;

public sealed class OrderSummaryInfo
{
    [Identity]
    public Guid Id { get; set; }

    public Status Status { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class OrderProjection : MultiStreamProjection<OrderSummaryInfo, Guid>
{
    public OrderProjection()
    {
        Name = nameof(OrderSummary);

        // Opt into 2nd level caching of up to 100
        // most recently encountered aggregates as a
        // performance optimization
        Options.CacheLimitPerTenant = 1000;

        // With large event stores of relatively small
        // event objects, moving this number up from the
        // default can greatly improve throughput and especially
        // improve projection rebuild times
        Options.BatchSize = 5000;

        // Tell the projection how to group the events
        // by the aggregate id
        Identity<DeleteBasketCompleteCommand>(e => e.Id);
        Identity<OrderCancelledEvent>(e => e.Order.Id);
        Identity<OrderCompletedEvent>(e => e.Order.Id);
    }

    public OrderSummaryInfo Create(OrderSummaryInfo info, DeleteBasketCompleteCommand @event)
    {
        info.Id = @event.OrderId;
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
