namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class DashboardExtensions
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string OtelExporterOtlpProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
    private const string OtelServiceName = "OTEL_SERVICE_NAME";

    /// <summary>
    ///     Adds the Aspire dashboard to the distributed application with comprehensive telemetry integration.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure with dashboard capabilities.</param>
    /// <remarks>
    ///     This method configures a containerized Aspire dashboard with the following features:
    ///     - Creates dashboard container with HTTP endpoints for web UI and OTLP telemetry ingestion
    ///     - Automatically configures OpenTelemetry environment variables for all application resources
    ///     - Sets up OTLP endpoint, protocol (gRPC), and service name for each resource
    ///     - Provides centralized monitoring and observability for the entire distributed application
    ///     - Uses Microsoft's official nightly Aspire dashboard container image
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var builder = DistributedApplication.CreateBuilder(args);
    ///
    ///     builder.AddDashboard();
    ///
    ///     builder.Build().Run();
    ///     </code>
    /// </example>
    public static void AddDashboard(this IDistributedApplicationBuilder builder)
    {
        var dashboard = builder
            .AddContainer(DashboardDefaults.ResourceName, DashboardDefaults.ContainerImageName)
            .WithHttpEndpoint(targetPort: DashboardDefaults.ContainerPort)
            .WithHttpEndpoint(name: Protocol.Otlp, targetPort: DashboardDefaults.OtlpPort);

        builder.Eventing.Subscribe<BeforeStartEvent>(
            (e, _) =>
            {
                foreach (var r in e.Model.Resources.OfType<IResourceWithEnvironment>())
                {
                    if (r == dashboard.Resource)
                    {
                        continue;
                    }

                    builder
                        .CreateResourceBuilder(r)
                        .WithEnvironment(c =>
                        {
                            c.EnvironmentVariables[OtelExporterOtlpEndpoint] =
                                dashboard.GetEndpoint(Protocol.Otlp);
                            c.EnvironmentVariables[OtelExporterOtlpProtocol] = Protocol.Grpc;
                            c.EnvironmentVariables[OtelServiceName] = r.Name;
                        });
                }

                return Task.CompletedTask;
            }
        );
    }

    private static class DashboardDefaults
    {
        public const string ContainerImageName =
            "mcr.microsoft.com/dotnet/nightly/aspire-dashboard";

        public const int ContainerPort = 18888;
        public const int OtlpPort = 18889;
        public const string ResourceName = "dashboard";
    }
}
