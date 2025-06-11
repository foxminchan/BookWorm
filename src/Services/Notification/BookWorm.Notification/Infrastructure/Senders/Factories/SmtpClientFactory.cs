using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using BookWorm.ServiceDefaults.Configuration;

namespace BookWorm.Notification.Infrastructure.Senders.Factories;

public sealed class SmtpClientFactory(IConfiguration configuration) : ISmtpClientFactory
{
    private readonly Lazy<UriBuilder> _smtpUri = new(() =>
        new(configuration.GetRequiredConnectionString(Components.MailPit))
    );

    public SmtpClient CreateClient()
    {
        var smtpClient = new SmtpClient();

        smtpClient.Connect(_smtpUri.Value.Host, _smtpUri.Value.Port);

        if (
            !string.Equals(
                _smtpUri.Value.Host,
                Restful.Host.Localhost,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            smtpClient.SslProtocols = SslProtocols.Tls13;
        }

        smtpClient.Authenticate(_smtpUri.Value.UserName, _smtpUri.Value.Password);

        return smtpClient;
    }
}
