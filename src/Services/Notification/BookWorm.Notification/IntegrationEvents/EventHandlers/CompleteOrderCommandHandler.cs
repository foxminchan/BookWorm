using System.Net.Mail;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class CompleteOrderCommandHandler(ISmtpClient smtpClient, EmailOptions emailOptions)
    : IConsumer<CompleteOrderCommand>
{
    [Channel("notification-complete-order")]
    [PublishOperation(
        typeof(CompleteOrderCommand),
        OperationId = nameof(CompleteOrderCommand),
        Summary = "Complete order notification"
    )]
    public async Task Consume(ConsumeContext<CompleteOrderCommand> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return;
        }

        const string subject = "Your order has been completed";
        var body =
            $"Your order with ID {message.OrderId} has been completed successfully. Total amount: {message.TotalMoney:C}";

        var mailMessage = new MailMessage(emailOptions.From, message.Email, subject, body)
        {
            IsBodyHtml = true,
        };

        await smtpClient.SendEmailAsync(mailMessage);
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
