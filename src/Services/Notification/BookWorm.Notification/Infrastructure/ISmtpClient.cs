using System.Net.Mail;

namespace BookWorm.Notification.Infrastructure;

public interface ISmtpClient
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken = default);
}
