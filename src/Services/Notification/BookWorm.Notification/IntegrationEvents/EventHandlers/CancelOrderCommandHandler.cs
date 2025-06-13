namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class CancelOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    EmailOptions emailOptions
) : IConsumer<CancelOrderCommand>
{
    [Channel("notification-cancel-order")]
    [PublishOperation(
        typeof(CancelOrderCommand),
        OperationId = nameof(CancelOrderCommand),
        Summary = "Cancel order notification",
        Description = "Represents a successful integration event when a user cancels an order"
    )]
    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        var order = message.ToOrder();

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithFrom(emailOptions)
            .WithTo(order.FullName, message.Email)
            .WithSubject(order)
            .WithBody(order, renderer)
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
