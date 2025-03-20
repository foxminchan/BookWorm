using System.Net.Mail;
using BookWorm.Notification.Exceptions;
using BookWorm.SharedKernel.OpenTelemetry;
using Polly.Registry;
using SendGrid.Helpers.Mail;

namespace BookWorm.Notification.Infrastructure;

public sealed class SendGridClient(
    ILogger<SendGridClient> logger,
    SendGirdOptions sendGirdOptions,
    ResiliencePipelineProvider<string> provider
) : ISmtpClient
{
    public async Task SendEmailAsync(
        MailMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = TelemetryTags.ActivitySource.StartActivity(
            $"{nameof(SmtpClientWithOpenTelemetry)}/{nameof(SendEmailAsync)}",
            ActivityKind.Client
        );

        activity?.SetTag(TelemetryTags.SmtpClient.Recipient, mailMessage.To.ToString());
        activity?.SetTag(TelemetryTags.SmtpClient.Subject, mailMessage.Subject);
        activity?.SetTag(TelemetryTags.SmtpClient.MessageId, mailMessage.Headers["Message-ID"]);
        activity?.SetTag(TelemetryTags.SmtpClient.EmailOperation, "Send");
        activity?.Propagate(mailMessage, InjectHeaderIntoMailMessage);

        var sendGridClient = new SendGrid.SendGridClient(sendGirdOptions.ApiKey);
        var message = new SendGridMessage
        {
            From = new(sendGirdOptions.SenderEmail, sendGirdOptions.SenderName),
            Subject = mailMessage.Subject,
            HtmlContent = mailMessage.Body,
        };

        foreach (var recipient in mailMessage.To)
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

    private void InjectHeaderIntoMailMessage(MailMessage message, string key, string value)
    {
        try
        {
            message.Headers.Add(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to inject header into mail message");
        }
    }
}
