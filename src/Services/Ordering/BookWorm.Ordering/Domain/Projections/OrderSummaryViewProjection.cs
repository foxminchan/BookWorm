using BookWorm.Contracts;
using Marten.Schema;

namespace BookWorm.Ordering.Domain.Projections;

public sealed class OrderSummaryView
{
    [Identity]
    public Guid Id { get; set; }

    public Status Status { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class OrderSummaryViewProjection : MultiStreamProjection<OrderSummaryView, Guid>
{
    public OrderSummaryViewProjection()
    {
        Name = nameof(OrderSummaryView);

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

    public static OrderSummaryView Create(OrderSummaryView view, DeleteBasketCompleteCommand @event)
    {
        view.Id = @event.OrderId;
        view.Status = Status.New;
        view.TotalPrice = @event.TotalMoney;
        return view;
    }

    public static OrderSummaryView Apply(OrderSummaryView view, OrderCancelledEvent @event)
    {
        view.Status = Status.Cancelled;
        view.TotalPrice = @event.Order.TotalPrice;
        return view;
    }

    public static OrderSummaryView Apply(OrderSummaryView view, OrderCompletedEvent @event)
    {
        view.Status = Status.Completed;
        view.TotalPrice = @event.Order.TotalPrice;
        return view;
    }
}
