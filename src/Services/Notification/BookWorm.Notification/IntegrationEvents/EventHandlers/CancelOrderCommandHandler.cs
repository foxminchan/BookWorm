using System.Net.Mail;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class CancelOrderCommandHandler(ISmtpClient smtpClient, EmailOptions emailOptions)
    : IConsumer<CancelOrderCommand>
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

        const string subject = "Your order has been canceled";
        var body =
            $"Your order with ID {message.OrderId} has been canceled. Total amount: {message.TotalMoney:C}";

        var mailMessage = new MailMessage(emailOptions.From, message.Email, subject, body)
        {
            IsBodyHtml = true,
        };

        await smtpClient.SendEmailAsync(mailMessage);
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
