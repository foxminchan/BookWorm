using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Domain.EventHandlers;

public sealed class OrderEventHandler(
    IDocumentSession documentSession,
    ILogger<OrderEventHandler> logger
)
    : INotificationHandler<OrderPlacedEvent>,
        INotificationHandler<OrderCompletedEvent>,
        INotificationHandler<OrderCancelledEvent>
{
    public async ValueTask Handle(
        OrderCancelledEvent notification,
        CancellationToken cancellationToken
    )
    {
        OrderingTrace.LogOrderCancelled(logger, notification.Order.Id, Status.Cancelled);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderCompletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        OrderingTrace.LogOrderCompleted(logger, notification.Order.Id, Status.Completed);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderPlacedEvent notification,
        CancellationToken cancellationToken
    )
    {
        OrderingTrace.LogOrderPlaced(logger, notification.Order.Id, Status.New);
        await documentSession.Add<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }
}
