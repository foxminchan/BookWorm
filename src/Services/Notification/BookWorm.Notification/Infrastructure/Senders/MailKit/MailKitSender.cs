﻿using Microsoft.Extensions.Diagnostics.Buffering;
using Polly.Registry;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

public sealed class MailKitSender(
    MailKitClientFactory factory,
    ILogger<MailKitSender> logger,
    GlobalLogBuffer logBuffer,
    ResiliencePipelineProvider<string> provider
) : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        var client = await factory.GetSmtpClientAsync(cancellationToken);

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
            logBuffer.Flush();
            throw new NotificationException($"Failed to send email. Exception: {ex.Message}", ex);
        }
    }
}
