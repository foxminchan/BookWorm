namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal sealed class MailKitClientFactory(MailKitSettings settings)
{
    /// <summary>
    ///     Creates a new connected SMTP client.
    ///     The caller is responsible for disposing the client after use.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A connected <see cref="ISmtpClient" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when connection or authentication fails.</exception>
    public async Task<SmtpClient> CreateClientAsync(CancellationToken cancellationToken = default)
    {
        var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(settings.Endpoint, cancellationToken);

            if (settings.Credentials is not null)
            {
                await client.AuthenticateAsync(settings.Credentials, cancellationToken);
            }

            return client;
        }
        catch (Exception ex)
        {
            client.Dispose();
            throw new InvalidOperationException(
                $"Failed to create SMTP client. Exception: {ex.Message}",
                ex
            );
        }
    }
}
