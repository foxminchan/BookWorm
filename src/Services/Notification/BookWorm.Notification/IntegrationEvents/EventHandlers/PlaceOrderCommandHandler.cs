using System.Net.Mail;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class PlaceOrderCommandHandler(ISmtpClient smtpClient, EmailOptions emailOptions)
    : IConsumer<PlaceOrderCommand>
{
    [Channel("notification-place-order")]
    [PublishOperation(
        typeof(PlaceOrderCommand),
        OperationId = nameof(PlaceOrderCommand),
        Summary = "Place order notification"
    )]
    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        const string subject = "Your order has been placed";
        var body =
            $"Your order with ID {message.OrderId} has been placed successfully. Total amount: {message.TotalMoney:C}";

        var mailMessage = new MailMessage(emailOptions.From, message.Email, subject, body)
        {
            IsBodyHtml = true,
        };

        await smtpClient.SendEmailAsync(mailMessage);
    }
}

public sealed class PlaceOrderCommandHandlerDefinition
    : ConsumerDefinition<PlaceOrderCommandHandler>
{
    public PlaceOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-place-order");
        ConcurrentMessageLimit = 1;
    }
}
