namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class CancelOrderCommandHandler(ISender sender, IRenderer renderer)
{
    [WolverineHandler]
    public Task Handle(CancelOrderCommand message, CancellationToken cancellationToken)
    {
        return OrderNotificationHelper.SendAsync(
            sender,
            renderer,
            message.ToOrder(),
            message.Email,
            cancellationToken
        );
    }
}
