using Polly.Registry;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BookWorm.Notification.Infrastructure.Senders.Providers;

public sealed class SendGridSender(
    ILogger<SendGridSender> logger,
    SendGridOptions sendGridOptions,
    ResiliencePipelineProvider<string> provider
) : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = TelemetryTags.ActivitySource.StartActivity(
            $"{nameof(MailKitSender)}/{nameof(SendAsync)}",
            ActivityKind.Client
        );

        activity?.SetTag(TelemetryTags.SmtpClient.Recipient, mailMessage.To.ToString());
        activity?.SetTag(TelemetryTags.SmtpClient.Subject, mailMessage.Subject);
        activity?.SetTag(TelemetryTags.SmtpClient.MessageId, mailMessage.Headers["Message-ID"]);
        activity?.SetTag(TelemetryTags.SmtpClient.EmailOperation, "Send");

        var sendGridClient = new SendGridClient(sendGridOptions.ApiKey);
        var message = new SendGridMessage
        {
            From = new(sendGridOptions.SenderEmail, sendGridOptions.SenderName),
            Subject = mailMessage.Subject,
            HtmlContent = mailMessage.HtmlBody,
        };

        foreach (var recipient in mailMessage.To.Mailboxes)
        {
            message.AddTo(new EmailAddress(recipient.Address));
        }

        var pipeline = provider.GetPipeline(nameof(Notification));

        var response = await pipeline.ExecuteAsync(
            async ct => await sendGridClient.SendEmailAsync(message, ct),
            cancellationToken
        );

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError(
                "Failed to send email to {Recipient} with subject {Subject}. Status code: {StatusCode}",
                mailMessage.To.ToString(),
                mailMessage.Subject,
                response.StatusCode
            );
            throw new NotificationException(
                $"Failed to send email. Status code: {response.StatusCode}"
            );
        }
    }
}
