namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

public sealed class MailKitClientFactory(MailKitSettings settings) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private SmtpClient? _client;

    public void Dispose()
    {
        _client?.Dispose();
        _semaphore.Dispose();
    }

    public async Task<ISmtpClient> GetSmtpClientAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_client is null || !_client.IsConnected)
            {
                _client = new();

                await _client
                    .ConnectAsync(settings.Endpoint, cancellationToken)
                    .ConfigureAwait(false);

                if (settings.Credentials is not null)
                {
                    await _client
                        .AuthenticateAsync(settings.Credentials, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            _client?.Dispose();
            _client = null;
            throw new InvalidOperationException(
                $"Failed to create SMTP client. Exception: {ex.Message}",
                ex
            );
        }
        finally
        {
            _semaphore.Release();
        }

        return _client;
    }
}
