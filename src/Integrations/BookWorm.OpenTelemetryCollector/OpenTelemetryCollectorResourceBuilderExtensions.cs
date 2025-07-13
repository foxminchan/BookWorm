using BookWorm.Constants.Aspire;

namespace BookWorm.OpenTelemetryCollector;

public static class OpenTelemetryCollectorResourceBuilderExtensions
{
    private const string DashboardOtlpUrlVariableName = "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL";
    private const string DashboardOtlpApiKeyVariableName = "AppHost:OtlpApiKey";
    private const string DashboardOtlpUrlDefaultValue = "http://localhost:18889";

    public static IResourceBuilder<OpenTelemetryCollectorResource> AddOpenTelemetryCollector(
        this IDistributedApplicationBuilder builder,
        string name,
        string configFileLocation
    )
    {
        builder.AddOpenTelemetryCollectorInfrastructure();

        var url =
            builder.Configuration[DashboardOtlpUrlVariableName] ?? DashboardOtlpUrlDefaultValue;
        var isHttpsEnabled = url.StartsWith(Protocol.Https, StringComparison.OrdinalIgnoreCase);

        var dashboardOtlpEndpoint = new HostUrl(url);

        var resource = new OpenTelemetryCollectorResource(name);
        var resourceBuilder = builder
            .AddResource(resource)
            .WithImage(
                OpenTelemetryCollectorDefaults.ContainerImageName,
                OpenTelemetryCollectorDefaults.ContainerImageTag
            )
            .WithImageRegistry(OpenTelemetryCollectorDefaults.ContainerRegistry)
            .WithEndpoint(
                targetPort: 4317,
                name: OpenTelemetryCollectorResource.OtlpGrpcEndpointName,
                scheme: isHttpsEnabled ? Protocol.Https : Protocol.Http
            )
            .WithEndpoint(
                targetPort: 4318,
                name: OpenTelemetryCollectorResource.OtlpHttpEndpointName,
                scheme: isHttpsEnabled ? Protocol.Https : Protocol.Http
            )
            .WithBindMount(configFileLocation, "/etc/otelcol-contrib/config.yaml")
            .WithEnvironment("ASPIRE_ENDPOINT", $"{dashboardOtlpEndpoint}")
            .WithEnvironment(
                "ASPIRE_API_KEY",
                builder.Configuration[DashboardOtlpApiKeyVariableName]
            )
            .WithEnvironment("ASPIRE_INSECURE", isHttpsEnabled ? "false" : "true");

        return resourceBuilder;
    }
}

public static class OpenTelemetryCollectorDefaults
{
    public const string ContainerRegistry = "ghcr.io";

    public const string ContainerImageName =
        "open-telemetry/opentelemetry-collector-releases/opentelemetry-collector-contrib";

    public const string ContainerImageTag = "0.129.0";
}
