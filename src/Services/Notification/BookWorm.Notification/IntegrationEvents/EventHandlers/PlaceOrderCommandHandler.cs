namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class PlaceOrderCommandHandler(ISender sender, IRenderer renderer)
    : IConsumer<PlaceOrderCommand>
{
    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        var order = message.ToOrder();

        var htmlBody = await renderer.RenderAsync(
            order,
            "Orders/OrderEmail",
            context.CancellationToken
        );

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, message.Email)
            .WithSubject(order)
            .WithBody(htmlBody)
            .Build();

        await sender.SendAsync(mailMessage, context.CancellationToken);
    }
}

[ExcludeFromCodeCoverage]
public sealed class PlaceOrderCommandHandlerDefinition
    : ConsumerDefinition<PlaceOrderCommandHandler>
{
    public PlaceOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-place-order");
        ConcurrentMessageLimit = 1;
    }
}
