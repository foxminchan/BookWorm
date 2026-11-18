using BookWorm.Chassis.Logging;
using Polly.Registry;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal sealed partial class MailKitSender(
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

            LogEmailSent(
                logger,
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
            throw new NotificationException(
                $"Failed to send email ({mailMessage.To.Mailboxes.Count()} recipient(s)) with subject {mailMessage.Subject}",
                ex
            );
        }
    }

    [LoggerMessage(
        EventId = 0,
        EventName = nameof(LogEmailSent),
        Level = LogLevel.Information,
        Message = "Email sent to {Recipients} with subject {Subject} (status {StatusCode}: {Response})"
    )]
    private static partial void LogEmailSent(
        ILogger logger,
        [PIIData] string recipients,
        string? subject,
        int statusCode,
        string response
    );
}
