using System.Diagnostics;

namespace BookWorm.Notification.Infrastructure;

public sealed class SmtpOutboxDecorator(
    ISmtpService smtpService,
    ILogger<SmtpOutboxDecorator> logger,
    IDocumentSession session) : ISmtpService
{
    public async Task SendEmailAsync(EmailMetadata emailMetadata, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[{Service}] Sending email to {To} with subject {Subject}",
            nameof(SmtpOutboxDecorator), emailMetadata.To, emailMetadata.Subject);

        using var activity = new ActivitySource("Smtp")
            .StartActivity($"Sending email to {emailMetadata.To} with subject {emailMetadata.Subject}",
                ActivityKind.Client);

        var emailOutbox = new EmailOutbox
        {
            Body = emailMetadata.Body, Subject = emailMetadata.Subject, To = emailMetadata.To, IsSent = false
        };

        if (activity is not null)
        {
            activity.AddTag("mail.to", emailMetadata.To);
            activity.AddTag("mail.subject", emailMetadata.Subject);
        }

        session.Store(emailOutbox);

        await session.SaveChangesAsync(cancellationToken);

        try
        {
            await smtpService.SendEmailAsync(emailMetadata, cancellationToken);
            emailOutbox.IsSent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Service}] Failed to send email to {To} with subject {Subject}",
                nameof(SmtpOutboxDecorator), emailMetadata.To, emailMetadata.Subject);

            if (activity is not null)
            {
                activity.AddTag("exception.message", ex.Message);
                activity.AddTag("exception.stacktrace", ex.ToString());
                activity.AddTag("exception.type", ex.GetType().FullName);
                activity.SetStatus(ActivityStatusCode.Error);
            }
        }
        finally
        {
            session.Update(emailOutbox);
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}
