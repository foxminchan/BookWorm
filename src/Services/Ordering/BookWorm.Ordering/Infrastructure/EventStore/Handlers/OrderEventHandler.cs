using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;
using Mediator;

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
        OrderingTrace.LogOrderCancelled(logger, notification.Order.Id, Status.Cancelled);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            activityScope,
            cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderCompletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        OrderingTrace.LogOrderCompleted(logger, notification.Order.Id, Status.Completed);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            activityScope,
            cancellationToken
        );
    }

    public async ValueTask Handle(
        OrderPlacedEvent notification,
        CancellationToken cancellationToken
    )
    {
        OrderingTrace.LogOrderPlaced(logger, notification.Order.Id, Status.New);
        documentSession.PropagateUserId(claimsPrincipal);
        await documentSession.Add<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            activityScope,
            cancellationToken
        );
    }
}
