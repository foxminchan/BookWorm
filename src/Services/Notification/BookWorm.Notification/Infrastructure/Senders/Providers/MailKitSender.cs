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
        var client = clientPool.Get();

        try
        {
            using var activity = TelemetryTags.ActivitySource.StartActivity(
                $"{nameof(MailKitSender)}/{nameof(SendAsync)}",
                ActivityKind.Client
            );

            activity?.SetTag(TelemetryTags.SmtpClient.Recipient, mailMessage.To.ToString());
            activity?.SetTag(TelemetryTags.SmtpClient.Subject, mailMessage.Subject);
            activity?.SetTag(TelemetryTags.SmtpClient.EmailOperation, "Send");
            if (!string.IsNullOrEmpty(mailMessage.Headers["Message-ID"]))
            {
                activity?.SetTag(
                    TelemetryTags.SmtpClient.MessageId,
                    mailMessage.Headers["Message-ID"]
                );
            }

            var pipeline = provider.GetPipeline(nameof(Notification));

            await pipeline.ExecuteAsync(
                async ct => await client.SendAsync(mailMessage, ct),
                cancellationToken
            );

            await client.DisconnectAsync(true, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to {Recipient} with subject {Subject}",
                mailMessage.To.ToString(),
                mailMessage.Subject
            );
            throw new NotificationException($"Failed to send email. Exception: {ex.Message}");
        }
        finally
        {
            clientPool.Return(client);
        }
    }
}
