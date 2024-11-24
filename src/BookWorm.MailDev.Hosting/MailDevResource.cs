namespace BookWorm.MailDev.Hosting;

/// <summary>
///     https://github.com/davidfowl/Build2024AspireDemo/blob/main/AppHost/MailDev/MailDevResource.cs
/// </summary>
/// <param name="name"></param>
public sealed class MailDevResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string SmtpEndpointName = "smtp";
    internal const string HttpEndpointName = "http";

    private EndpointReference? _smtpReference;

    public EndpointReference SmtpEndpoint => _smtpReference ??= new(this, SmtpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"smtp://{SmtpEndpoint.Property(EndpointProperty.Host)}:{SmtpEndpoint.Property(EndpointProperty.Port)}"
        );
}
