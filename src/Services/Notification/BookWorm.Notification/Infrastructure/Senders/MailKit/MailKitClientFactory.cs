namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal sealed class MailKitClientFactory(MailKitSettings settings)
{
    /// <summary>
    ///     Creates a new connected SMTP client.
    ///     The caller is responsible for disposing the client after use.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A connected <see cref="ISmtpClient" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the SMTP endpoint is not configured.</exception>
    public async Task<SmtpClient> CreateClientAsync(CancellationToken cancellationToken = default)
    {
        if (settings.Endpoint is null)
        {
            throw new InvalidOperationException("SMTP endpoint is not configured.");
        }

        var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(settings.Endpoint, cancellationToken);

            if (settings is { Username: { } username, Password: { } password })
            {
                await client.AuthenticateAsync(username, password, cancellationToken);
            }

            return client;
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }
    }
}
