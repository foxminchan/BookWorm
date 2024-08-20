using BookWorm.Notification.Extensions;
using BookWorm.Notification.OpenTelemetry;

namespace BookWorm.Notification.Infrastructure;

public sealed class SmtpOutboxDecorator(
    ISmtpService smtpService,
    ILogger<SmtpOutboxDecorator> logger,
    IDocumentSession session) : ISmtpService
{
    public async Task SendEmailAsync(EmailMetadata emailMetadata, CancellationToken cancellationToken = default)
    {
        NotificationTrace.LogEmailSending(logger, nameof(SmtpOutboxDecorator), emailMetadata.To, emailMetadata.Subject);

        using var emailActivity = new ActivitySource(SmtpTelemetry.ActivityName)
            .StartActivity($"Sending email to {emailMetadata.To} with subject {emailMetadata.Subject}",
                ActivityKind.Client);

        using var martenActivity = new ActivitySource(MartenTelemetry.ActivityName)
            .StartActivity($"Storing email to {emailMetadata.To} with subject {emailMetadata.Subject}");

        emailActivity?.AddTag("mail.to", emailMetadata.To);
        emailActivity?.AddTag("mail.subject", emailMetadata.Subject);

        var emailOutbox = new EmailOutbox
        {
            Body = emailMetadata.Body, Subject = emailMetadata.Subject, To = emailMetadata.To, IsSent = false
        };

        try
        {
            await StoreEmailOutboxAsync(emailOutbox, martenActivity, cancellationToken);
            await SendEmailAndMarkAsSentAsync(emailMetadata, emailOutbox, emailActivity, cancellationToken);
        }
        catch (Exception ex)
        {
            emailActivity?.SetStatus(ActivityStatusCode.Error);
            martenActivity?.SetStatus(ActivityStatusCode.Error);
            NotificationTrace.LogEmailFailed(logger, nameof(SmtpOutboxDecorator), emailMetadata.To,
                ex.Message);
        }
        finally
        {
            await UpdateEmailOutboxAsync(emailOutbox, martenActivity, cancellationToken);
        }
    }

    private async Task StoreEmailOutboxAsync(EmailOutbox emailOutbox, Activity? martenActivity,
        CancellationToken cancellationToken)
    {
        try
        {
            session.Store(emailOutbox);
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (martenActivity is not null)
            {
                TagException(ex, martenActivity);
            }
        }
    }

    private async Task SendEmailAndMarkAsSentAsync(EmailMetadata emailMetadata, EmailOutbox emailOutbox,
        Activity? emailActivity, CancellationToken cancellationToken)
    {
        try
        {
            await smtpService.SendEmailAsync(emailMetadata, cancellationToken);
            emailOutbox.IsSent = true;
        }
        catch (Exception ex)
        {
            if (emailActivity is not null)
            {
                TagException(ex, emailActivity);
            }
        }
    }

    private async Task UpdateEmailOutboxAsync(EmailOutbox emailOutbox, Activity? martenActivity,
        CancellationToken cancellationToken)
    {
        try
        {
            session.Update(emailOutbox);
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (martenActivity is not null)
            {
                TagException(ex, martenActivity);
            }

            logger.LogError(ex, "[{Event}] - Failed to update email outbox", nameof(SmtpOutboxDecorator));
        }
    }

    private static void TagException(Exception ex, Activity activity)
    {
        activity.AddTag("exception.message", ex.Message);
        activity.AddTag("exception.stacktrace", ex.ToString());
        activity.AddTag("exception.type", ex.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
