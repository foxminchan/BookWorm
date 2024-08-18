namespace BookWorm.Notification.Infrastructure;

public interface ISmtpService
{
    Task SendEmailAsync(EmailMetadata emailMetadata, CancellationToken cancellationToken = default);
}
