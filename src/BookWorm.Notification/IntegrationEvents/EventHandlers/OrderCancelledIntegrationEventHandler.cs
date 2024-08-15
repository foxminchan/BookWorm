using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.IntegrationEvents.Events;
using BookWorm.Notification.Models;
using MassTransit;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class OrderCancelledIntegrationEventHandler(ISmtpService smtpService)
    : IConsumer<OrderCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        if (context.Message.Email is null)
        {
            return;
        }

        var metadata = new EmailMetadata(
            context.Message.Email,
            "Order Cancelled",
            $"Your order has been cancelled. Order ID: {context.Message.OrderId}");

        await smtpService.SendEmailAsync(metadata);
    }
}
