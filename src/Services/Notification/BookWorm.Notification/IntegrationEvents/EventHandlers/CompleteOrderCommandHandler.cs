namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class CompleteOrderCommandHandler(ISender sender, IRenderer renderer)
    : IConsumer<CompleteOrderCommand>
{
    public async Task Consume(ConsumeContext<CompleteOrderCommand> context)
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
public sealed class CompleteOrderCommandHandlerDefinition
    : ConsumerDefinition<CompleteOrderCommandHandler>
{
    public CompleteOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-complete-order");
        ConcurrentMessageLimit = 1;
    }
}
