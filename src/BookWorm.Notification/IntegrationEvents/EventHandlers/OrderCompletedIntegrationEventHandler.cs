using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.IntegrationEvents.Events;
using BookWorm.Notification.Models;
using MassTransit;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class OrderCompletedIntegrationEventHandler(ISmtpService smtpService)
    : IConsumer<OrderCompletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        if (context.Message.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            context.Message.Email,
            "Order Completed",
            $"Your order has been completed. Order ID: {context.Message.OrderId}");

        await smtpService.SendEmailAsync(metadata);
    }
}
