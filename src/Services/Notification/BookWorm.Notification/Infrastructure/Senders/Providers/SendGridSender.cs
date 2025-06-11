using BookWorm.Notification.Infrastructure.Senders.Extensions;
using Polly.Registry;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BookWorm.Notification.Infrastructure.Senders.Providers;

public sealed class SendGridSender(
    ILogger<SendGridSender> logger,
    SendGridOptions sendGridOptions,
    ISendGridClient sendGridClient,
    ResiliencePipelineProvider<string> provider
) : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        await this.WithTelemetry(
            mailMessage,
            async token => await SendEmailAsync(mailMessage, token),
            cancellationToken
        );
    }

    private async Task SendEmailAsync(MimeMessage mailMessage, CancellationToken cancellationToken)
    {
        var message = new SendGridMessage
        {
            From = new(sendGridOptions.SenderEmail, sendGridOptions.SenderName),
            Subject = mailMessage.Subject,
            HtmlContent = mailMessage.HtmlBody,
        };

        foreach (var recipient in mailMessage.To.Mailboxes)
        {
            message.AddTo(new EmailAddress(recipient.Address, recipient.Name ?? string.Empty));
        }

        var pipeline = provider.GetPipeline(nameof(Notification));

        var response = await pipeline.ExecuteAsync(
            async ct => await sendGridClient.SendEmailAsync(message, ct),
            cancellationToken
        );

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Accepted))
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
