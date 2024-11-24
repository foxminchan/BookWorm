using BookWorm.Ordering.Domain.OrderAggregate.Events;
using BookWorm.Ordering.Infrastructure.Marten;

namespace BookWorm.Ordering.Features.Orders.EventHandlers;

public sealed class OrderEventHandler(
    IDocumentSession documentSession,
    ILogger<OrderEventHandler> logger
)
    : INotificationHandler<OrderCreatedEvent>,
        INotificationHandler<OrderCompletedEvent>,
        INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCreated(logger, nameof(OrderCancelledEvent), notification.Id);
        await documentSession.GetAndUpdate<OrderState>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCompleted(logger, nameof(OrderCompletedEvent), notification.Id);
        await documentSession.GetAndUpdate<OrderState>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCreated(logger, nameof(OrderCreatedEvent), notification.Id);
        await documentSession.GetAndUpdate<OrderState>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }
}
