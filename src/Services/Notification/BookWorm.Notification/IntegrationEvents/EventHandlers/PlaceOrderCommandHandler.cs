using BookWorm.Chassis.EventBus.Idempotency;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(PlaceOrderCommand message, CancellationToken cancellationToken)
    {
        await idempotencyGuard.ExecuteAsync(
            message.Id,
            nameof(PlaceOrderCommandHandler),
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
