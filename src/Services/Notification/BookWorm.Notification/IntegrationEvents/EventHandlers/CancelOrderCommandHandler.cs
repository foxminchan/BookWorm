using BookWorm.Chassis.EventBus.Idempotency;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class CancelOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(CancelOrderCommand message, CancellationToken cancellationToken)
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                message.Id,
                nameof(CancelOrderCommandHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return;
        }

        await OrderNotificationHelper.SendAsync(
            sender,
            renderer,
            message.ToOrder(),
            message.Email,
            cancellationToken
        );
    }
}
