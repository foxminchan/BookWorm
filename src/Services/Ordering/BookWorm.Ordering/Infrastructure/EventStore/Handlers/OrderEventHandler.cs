using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

namespace BookWorm.Ordering.Infrastructure.EventStore.Handlers;

internal sealed class OrderEventHandler(
    IDocumentSession documentSession,
    IActivityScope activityScope,
    ClaimsPrincipal claimsPrincipal,
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
        var orderId = (Guid)notification.Order.Id;
        OrderingTrace.LogOrderCancelled(logger, orderId, Status.Cancelled);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.GetAndUpdate<OrderSummary>(
            orderId,
            notification,
            activityScope,
            cancellationToken: cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderCompletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)notification.Order.Id;
        OrderingTrace.LogOrderCompleted(logger, orderId, Status.Completed);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.GetAndUpdate<OrderSummary>(
            orderId,
            notification,
            activityScope,
            cancellationToken: cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderPlacedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)notification.Order.Id;
        OrderingTrace.LogOrderPlaced(logger, orderId, Status.New);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.Add<OrderSummary>(
            orderId,
            notification,
            activityScope,
            cancellationToken
        );
    }
}
