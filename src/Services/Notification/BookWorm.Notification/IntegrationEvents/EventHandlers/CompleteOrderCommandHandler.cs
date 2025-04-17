﻿namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class CompleteOrderCommandHandler(
    ISender sender,
    IRenderer renderer,
    EmailOptions emailOptions
) : IConsumer<CompleteOrderCommand>
{
    [Channel("notification-complete-order")]
    [PublishOperation(
        typeof(CompleteOrderCommand),
        OperationId = nameof(CompleteOrderCommand),
        Summary = "Complete order notification",
        Description = "Represents a successful integration event when a user completes an order"
    )]
    public async Task Consume(ConsumeContext<CompleteOrderCommand> context)
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
public sealed class CompleteOrderCommandHandlerDefinition
    : ConsumerDefinition<CompleteOrderCommandHandler>
{
    public CompleteOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-complete-order");
        ConcurrentMessageLimit = 1;
    }
}
