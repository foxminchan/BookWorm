using BookWorm.Contracts;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class OrderCompletedIntegrationEventHandler(
    ISmtpService smtpService,
    ILogger<OrderCompletedIntegrationEventHandler> logger) : IConsumer<OrderCompletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] Sending email to {Email} for order {OrderId}",
            nameof(OrderCompletedIntegrationEventHandler), @event.Email, @event.OrderId);

        if (@event.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            @event.Email,
            "Order Completed",
            $"Your order has been completed. Order ID: {@event.OrderId}");

        await smtpService.SendEmailAsync(metadata);
    }
}

internal sealed class OrderCompletedIntegrationEventHandlerDefinition
    : ConsumerDefinition<OrderCompletedIntegrationEventHandler>
{
    public OrderCompletedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "order-completed");
        ConcurrentMessageLimit = 1;
    }
}
