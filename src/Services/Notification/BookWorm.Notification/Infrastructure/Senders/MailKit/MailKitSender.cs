using Polly.Registry;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal sealed class MailKitSender(
    MailKitClientFactory factory,
    ILogger<MailKitSender> logger,
    MailKitSettings settings,
    ResiliencePipelineProvider<string> provider
) : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        if (mailMessage.From.Count == 0)
        {
            mailMessage.From.Add(new MailboxAddress(settings.Name, settings.From));
        }

        var request = new SmtpSendRequest(mailMessage);
        var pipeline = provider.GetPipeline(nameof(Notification));

        try
        {
            var result = await pipeline.ExecuteAsync(
                async ct =>
                {
                    await using var client = await factory.CreateClientAsync(ct);
                    var sendResult = (SmtpSendResult)await client.SendAsync(request, ct);
                    await client.DisconnectAsync(true, ct);
                    return sendResult;
                },
                cancellationToken
            );

            logger.LogInformation(
                "Email sent to {Recipients} with subject {Subject} (status {StatusCode}: {Response})",
                string.Join(", ", result.AcceptedRecipients.Select(r => r.Address)),
                mailMessage.Subject,
                (int)result.StatusCode,
                result.ResponseText
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var recipients = string.Join(", ", mailMessage.To.Mailboxes.Select(m => m.Address));
            throw new NotificationException(
                $"Failed to send email to {recipients} with subject {mailMessage.Subject}",
                ex
            );
        }
    }
}
