namespace BookWorm.Notification.Infrastructure.Senders.Factories;

public interface ISmtpClientFactory
{
    SmtpClient CreateClient();
}
