using System.Net.Mail;
using BookWorm.Notification.Exceptions;
using BookWorm.SharedKernel.OpenTelemetry;
using SendGrid.Helpers.Mail;

namespace BookWorm.Notification.Infrastructure;

public sealed class SendGridClient(ILogger<SendGridClient> logger, SendGirdOptions sendGirdOptions)
    : ISmtpClient
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
        var sendGridMessage = new SendGridMessage
        {
            From = new(sendGirdOptions.SenderEmail, sendGirdOptions.SenderName),
            Subject = mailMessage.Subject,
            HtmlContent = mailMessage.Body,
        };

        foreach (var recipient in mailMessage.To)
        {
            sendGridMessage.AddTo(new EmailAddress(recipient.Address));
        }

        var response = await sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);

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
