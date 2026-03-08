namespace Aspire.Hosting.ApplicationModel;

public sealed class PresidioAnalyzerResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";

    /// <summary>
    ///     Gets the HTTP endpoint reference for the Presidio Analyzer service.
    /// </summary>
    private EndpointReference HttpEndpoint => field ??= new(this, HttpEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Presidio Analyzer service.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );
}

public sealed class PresidioAnonymizerResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";

    /// <summary>
    ///     Gets the HTTP endpoint reference for the Presidio Anonymizer service.
    /// </summary>
    private EndpointReference HttpEndpoint => field ??= new(this, HttpEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Presidio Anonymizer service.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );
}
