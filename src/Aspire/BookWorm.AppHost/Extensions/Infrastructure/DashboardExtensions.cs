﻿namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class DashboardExtensions
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string OtelExporterOtlpProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
    private const string OtelServiceName = "OTEL_SERVICE_NAME";

    /// <summary>
    ///     Adds the Aspire dashboard to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <remarks>
    ///     The dashboard is only added in publishing mode.
    ///     It provides monitoring and telemetry
    ///     capabilities for the distributed application by configuring OpenTelemetry (OTLP)
    ///     endpoints for all resources.
    /// </remarks>
    public static void AddDashboard(this IDistributedApplicationBuilder builder)
    {
        if (!builder.ExecutionContext.IsPublishMode)
        {
            return;
        }

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
