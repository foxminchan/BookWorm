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
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                message.Id,
                nameof(CompleteOrderCommandHandler),
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
