namespace Aspire.Hosting.ApplicationModel;

public sealed class PresidioAnalyzerResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string HttpsEndpointName = "https";

    /// <summary>
    ///     Gets the HTTPS endpoint reference for the Presidio Analyzer service.
    /// </summary>
    private EndpointReference HttpsEndpoint => field ??= new(this, HttpsEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Presidio Analyzer service.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpsEndpointName}://{HttpsEndpoint.Property(EndpointProperty.Host)}:{HttpsEndpoint.Property(EndpointProperty.Port)}"
        );
}

public sealed class PresidioAnonymizerResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string HttpsEndpointName = "https";

    /// <summary>
    ///     Gets the HTTPS endpoint reference for the Presidio Anonymizer service.
    /// </summary>
    private EndpointReference HttpsEndpoint => field ??= new(this, HttpsEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Presidio Anonymizer service.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpsEndpointName}://{HttpsEndpoint.Property(EndpointProperty.Host)}:{HttpsEndpoint.Property(EndpointProperty.Port)}"
        );
}
