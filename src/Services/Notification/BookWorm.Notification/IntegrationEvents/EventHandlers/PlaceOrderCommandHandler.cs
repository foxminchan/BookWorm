namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class PlaceOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    EmailOptions emailOptions
) : IConsumer<PlaceOrderCommand>
{
    [Channel("notification-place-order")]
    [PublishOperation(
        typeof(PlaceOrderCommand),
        OperationId = nameof(PlaceOrderCommand),
        Summary = "Place order notification",
        Description = "Represents a successful integration event when a user places an order"
    )]
    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        var order = message.ToOrder();

        var mailMessage = new OrderMimeMessageBuilder()
            .WithFrom(emailOptions)
            .WithTo(order.FullName, message.Email)
            .WithSubject(order)
            .WithBody(order, renderer)
            .Build();

        await sender.SendAsync(mailMessage);
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
