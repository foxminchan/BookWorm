namespace BookWorm.Notification.Infrastructure.Senders;

internal interface ISender
{
    Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default);
}
