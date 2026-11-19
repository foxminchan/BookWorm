namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class CompleteOrderCommandHandler(ISender sender, IRenderer renderer)
{
    [WolverineHandler]
    public Task Handle(CompleteOrderCommand message, CancellationToken cancellationToken)
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
