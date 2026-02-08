namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class CancelOrderCommandHandler(ISender sender, IRenderer renderer)
    : IConsumer<CancelOrderCommand>
{
    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
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
public sealed class CancelOrderCommandHandlerDefinition
    : ConsumerDefinition<CancelOrderCommandHandler>
{
    public CancelOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-cancel-order");
        ConcurrentMessageLimit = 1;
    }
}
