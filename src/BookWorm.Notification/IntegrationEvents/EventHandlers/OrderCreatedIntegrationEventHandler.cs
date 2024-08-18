using BookWorm.Notification.IntegrationEvents.Events;
using MassTransit;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class OrderCreatedIntegrationEventHandler(ISmtpService smtpService)
    : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        if (context.Message.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            context.Message.Email,
            "Order Created",
            $"Thank you for your order. Order will be processed soon. Order ID: {context.Message.OrderId}");

        await smtpService.SendEmailAsync(metadata);
    }
}
