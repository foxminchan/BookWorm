using BookWorm.Notification.Infrastructure.Senders.Extensions;
using Polly.Registry;

namespace BookWorm.Notification.Infrastructure.Senders.Providers;

public sealed class MailKitSender(
    ObjectPool<SmtpClient> clientPool,
    ILogger<MailKitSender> logger,
    ResiliencePipelineProvider<string> provider
) : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        await this.WithTelemetry(
            mailMessage,
            async token => await SendEmailAsync(mailMessage, token),
            cancellationToken
        );
    }

    private async Task SendEmailAsync(MimeMessage mailMessage, CancellationToken cancellationToken)
    {
        var client = clientPool.Get();

        try
        {
            var pipeline = provider.GetPipeline(nameof(Notification));

            await pipeline.ExecuteAsync(
                async ct => await client.SendAsync(mailMessage, ct),
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to {Recipient} with subject {Subject}",
                mailMessage.To.ToString(),
                mailMessage.Subject
            );
            throw new NotificationException($"Failed to send email. Exception: {ex.Message}", ex);
        }
        finally
        {
            await DisconnectAndReturnClient(client, mailMessage, cancellationToken);
        }
    }

    private async Task DisconnectAndReturnClient(
        SmtpClient client,
        MimeMessage mailMessage,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to disconnect SMTP client after sending email to {Recipient} with subject {Subject}",
                mailMessage.To.ToString(),
                mailMessage.Subject
            );
        }

        clientPool.Return(client);
    }
}
