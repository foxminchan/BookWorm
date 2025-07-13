using BookWorm.OpenTelemetryCollector;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class GrafanaDashboardExtensions
{
    public static IResourceBuilder<ContainerResource> AddGrafanaDashboard(
        this IDistributedApplicationBuilder builder
    )
    {
        var prometheus = builder.AddPrometheus();

        builder
            .AddOtelCollector()
            .WithEnvironment(
                "PROMETHEUS_ENDPOINT",
                ReferenceExpression.Create($"{prometheus.GetEndpoint(Protocol.Http)}/api/v1/otlp")
            );

        var grafana = builder
            .AddContainer("grafana", "grafana/grafana", "12.0.2")
            .WithBindMount("Container/grafana/config", "/etc/grafana", isReadOnly: true)
            .WithBindMount(
                "Container/grafana/dashboards",
                "/var/lib/grafana/dashboards",
                isReadOnly: true
            )
            .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint(Protocol.Http))
            .WithHttpEndpoint(targetPort: 3000, name: Protocol.Http);

        return grafana;
    }

    private static IResourceBuilder<ContainerResource> AddPrometheus(
        this IDistributedApplicationBuilder builder
    )
    {
        var prometheus = builder
            .AddContainer("prometheus", "prom/prometheus", "v3.4.2")
            .WithBindMount("Container/prometheus", "/etc/prometheus", isReadOnly: true)
            .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
            .WithHttpEndpoint(targetPort: 9090, name: Protocol.Http);

        return prometheus;
    }

    private static IResourceBuilder<OpenTelemetryCollectorResource> AddOtelCollector(
        this IDistributedApplicationBuilder builder
    )
    {
        var otel = builder.AddOpenTelemetryCollector(
            "otelcollector",
            "Container/otelcollector/config.yaml"
        );

        if (builder.IsHttpsLaunchProfile() && builder.ExecutionContext.IsRunMode)
        {
            otel.RunWithHttpsDevCertificate(
                "HTTPS_CERT_FILE",
                "HTTPS_CERT_KEY_FILE",
                (_, _) =>
                {
                    // Set TLS details using YAML path via the command line. This allows the values to be added to the existing config file.
                    // Setting the values in the config file doesn't work because adding the "tls" section always enables TLS, even if there is no cert provided.
                    otel.WithArgs(
                        """
                        --config=yaml:receivers::otlp::protocols::grpc::tls::cert_file: "dev-certs/dev-cert.pem"
                        """,
                        """
                        --config=yaml:receivers::otlp::protocols::grpc::tls::key_file: "dev-certs/dev-cert.key"
                        """,
                        "--config=/etc/otelcol-contrib/config.yaml"
                    );
                }
            );
        }

        return otel;
    }
}
