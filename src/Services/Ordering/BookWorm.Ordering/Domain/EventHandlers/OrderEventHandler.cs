using BookWorm.Contracts;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Infrastructure.EventStore.DocumentSession;
using Saunter.Attributes;

namespace BookWorm.Ordering.Domain.EventHandlers;

[AsyncApi]
public sealed class OrderEventHandler(
    IEventDispatcher eventDispatcher,
    IDocumentSession documentSession,
    ILogger<OrderEventHandler> logger
)
    : INotificationHandler<OrderPlacedEvent>,
        INotificationHandler<OrderCompletedEvent>,
        INotificationHandler<OrderCancelledEvent>
{
    [Channel($"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(UserCheckedOutIntegrationEvent)}")]
    [SubscribeOperation(
        typeof(UserCheckedOutIntegrationEvent),
        OperationId = nameof(UserCheckedOutIntegrationEvent),
        Summary = "User checked out",
        Description = "Represents a successful integration event when a user checks out"
    )]
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCancelled(logger, notification.Order.Id, Status.New);
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    [Channel(
        $"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(OrderStatusChangedToCompleteIntegrationEvent)}"
    )]
    [SubscribeOperation(
        typeof(OrderStatusChangedToCompleteIntegrationEvent),
        OperationId = nameof(OrderStatusChangedToCompleteIntegrationEvent),
        Summary = "Order status changed to complete",
        Description = "Represents a successful integration event when an order status changes to complete"
    )]
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCompleted(logger, notification.Order.Id, Status.New);
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    [Channel(
        $"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(OrderStatusChangedToCancelIntegrationEvent)}"
    )]
    [SubscribeOperation(
        typeof(OrderStatusChangedToCancelIntegrationEvent),
        OperationId = nameof(OrderStatusChangedToCancelIntegrationEvent),
        Summary = "Order status changed to cancel",
        Description = "Represents a successful integration event when an order status changes to cancel"
    )]
    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderPlaced(logger, notification.Order.Id, Status.New);
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
    }
}
