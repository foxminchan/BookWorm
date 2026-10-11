namespace BookWorm.Notification.Infrastructure.Senders;

internal interface IEmailSender
{
    Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default);
}
