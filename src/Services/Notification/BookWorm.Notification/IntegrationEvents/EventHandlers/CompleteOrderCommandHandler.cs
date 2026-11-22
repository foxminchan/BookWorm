using BookWorm.Chassis.EventBus.Idempotency;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class CompleteOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(CompleteOrderCommand message, CancellationToken cancellationToken)
    {
        await idempotencyGuard.ExecuteAsync(
            message.Id,
            nameof(CompleteOrderCommandHandler),
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
