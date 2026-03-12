namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class CancelOrderCommandHandler(ISender sender, IRenderer renderer)
{
    public async Task Handle(CancelOrderCommand message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        var order = message.ToOrder();

        var htmlBody = await renderer.RenderAsync(order, "Orders/OrderEmail", cancellationToken);

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, message.Email)
            .WithSubject(order)
            .WithBody(htmlBody)
            .Build();

        await sender.SendAsync(mailMessage, cancellationToken);
    }
}
