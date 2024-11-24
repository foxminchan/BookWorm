using Polly.Registry;

namespace BookWorm.Notification.Infrastructure;

public sealed class SmtpService(
    IFluentEmailFactory factory,
    ResiliencePipelineProvider<string> pipeline
) : ISmtpService
{
    public async Task SendEmailAsync(
        EmailMetadata emailMetadata,
        CancellationToken cancellationToken = default
    )
    {
        var email = factory.Create();

        email = email.To(emailMetadata.To).Subject(emailMetadata.Subject).Body(emailMetadata.Body);

        var policy = pipeline.GetPipeline(nameof(Email));

        await policy.ExecuteAsync(async token => await email.SendAsync(token), cancellationToken);
    }
}
