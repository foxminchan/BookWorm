using BookWorm.Contracts;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class OrderCreatedIntegrationEventHandler(
    ISmtpService smtpService,
    ILogger<OrderCreatedIntegrationEventHandler> logger
) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "[{Consumer}] Sending email to {Email} for order {OrderId}",
            nameof(OrderCreatedIntegrationEventHandler),
            @event.Email,
            @event.OrderId
        );

        if (@event.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            @event.Email,
            "Order Created",
            $"Thank you for your order. Order will be processed soon. Order ID: {@event.OrderId}"
        );

        await smtpService.SendEmailAsync(metadata);
    }
}

internal sealed class OrderCreatedIntegrationEventHandlerDefinition
    : ConsumerDefinition<OrderCreatedIntegrationEventHandler>
{
    public OrderCreatedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "order-created");
        ConcurrentMessageLimit = 1;
    }
}
