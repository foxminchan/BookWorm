namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(ISender sender, IRenderer renderer)
{
    [WolverineHandler]
    public Task Handle(PlaceOrderCommand message, CancellationToken cancellationToken)
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
