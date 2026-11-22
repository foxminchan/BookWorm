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
        await idempotencyGuard.ExecuteAsync(
            message.Id,
            nameof(CancelOrderCommandHandler),
            ct =>
                OrderNotificationHelper.SendAsync(
                    sender,
                    renderer,
                    message.ToOrder(),
                    message.Email,
                    ct
                ),
            cancellationToken: cancellationToken
        );
    }
}
