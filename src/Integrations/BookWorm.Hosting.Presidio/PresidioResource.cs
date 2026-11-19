namespace Aspire.Hosting.ApplicationModel;

public abstract class PresidioResource(string name)
    : ContainerResource(name),
        IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";
    internal const string HttpsEndpointName = "https";

    private EndpointReference HttpEndpoint => field ??= new(this, HttpEndpointName);
    private EndpointReference HttpsEndpoint => field ??= new(this, HttpsEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Presidio service.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        Annotations.OfType<EndpointAnnotation>().Any(e => e.Name == HttpsEndpointName)
            ? ReferenceExpression.Create(
                $"{HttpsEndpointName}://{HttpsEndpoint.Property(EndpointProperty.Host)}:{HttpsEndpoint.Property(EndpointProperty.Port)}"
            )
            : ReferenceExpression.Create(
                $"{HttpEndpointName}://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
            );
}

public sealed class PresidioAnalyzerResource(string name) : PresidioResource(name);

public sealed class PresidioAnonymizerResource(string name) : PresidioResource(name);
