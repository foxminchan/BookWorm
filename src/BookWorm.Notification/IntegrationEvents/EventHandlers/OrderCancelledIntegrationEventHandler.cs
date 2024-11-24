using BookWorm.Contracts;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class OrderCancelledIntegrationEventHandler(
    ISmtpService smtpService,
    ILogger<OrderCancelledIntegrationEventHandler> logger
) : IConsumer<OrderCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "[{Consumer}] Sending email to {Email} for order {OrderId}",
            nameof(OrderCancelledIntegrationEventHandler),
            @event.Email,
            @event.OrderId
        );

        if (@event.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            @event.Email,
            "Order Cancelled",
            $"Your order has been cancelled. Order ID: {@event.OrderId}"
        );

        await smtpService.SendEmailAsync(metadata);
    }
}

internal sealed class OrderCancelledIntegrationEventHandlerDefinition
    : ConsumerDefinition<OrderCancelledIntegrationEventHandler>
{
    public OrderCancelledIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "order-cancelled");
        ConcurrentMessageLimit = 1;
    }
}
