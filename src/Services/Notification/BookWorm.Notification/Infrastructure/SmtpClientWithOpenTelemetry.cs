using System.Net.Mail;
using BookWorm.Notification.Exceptions;
using BookWorm.SharedKernel.OpenTelemetry;

namespace BookWorm.Notification.Infrastructure;

public sealed class SmtpClientWithOpenTelemetry(
    ObjectPool<SmtpClient> clientPool,
    ILogger<SmtpClientWithOpenTelemetry> logger
) : ISmtpClient
{
    public async Task SendEmailAsync(
        MailMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        var client = clientPool.Get();

        try
        {
            using var activity = TelemetryTags.ActivitySource.StartActivity(
                $"{nameof(SmtpClientWithOpenTelemetry)}/{nameof(SendEmailAsync)}",
                ActivityKind.Client
            );

            activity?.SetTag(TelemetryTags.SmtpClient.Recipient, mailMessage.To.ToString());
            activity?.SetTag(TelemetryTags.SmtpClient.Subject, mailMessage.Subject);
            if (!string.IsNullOrEmpty(mailMessage.Headers["Message-ID"]))
            {
                activity?.SetTag(
                    TelemetryTags.SmtpClient.MessageId,
                    mailMessage.Headers["Message-ID"]
                );
            }

            activity?.SetTag(TelemetryTags.SmtpClient.EmailOperation, "Send");

            activity.Propagate(mailMessage, InjectHeaderIntoMailMessage);

            await client.SendMailAsync(mailMessage, cancellationToken);

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

    private void InjectHeaderIntoMailMessage(MailMessage message, string key, string value)
    {
        try
        {
            message.Headers.Add(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to inject trace context header: {Key}", key);
        }
    }
}
