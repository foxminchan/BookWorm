using BookWorm.OpenTelemetryCollector;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class GrafanaDashboardExtensions
{
    /// <summary>
    ///     Adds a Grafana dashboard container to the distributed application.
    ///     Configures Prometheus and OpenTelemetry Collector resources,
    ///     sets up environment variables and bind mounts for Grafana,
    ///     and exposes the HTTP endpoint for Grafana.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>
    ///     An <see cref="IResourceBuilder{ContainerResource}" /> for the Grafana container.
    /// </returns>
    public static IResourceBuilder<ContainerResource> AddGrafanaDashboard(
        this IDistributedApplicationBuilder builder
    )
    {
        var prometheus = builder.AddPrometheus();

        var otel = builder
            .AddOtelCollector()
            .WithEnvironment(
                "PROMETHEUS_ENDPOINT",
                ReferenceExpression.Create($"{prometheus.GetEndpoint(Protocol.Http)}/api/v1/otlp")
            );

        var grafana = builder
            .AddContainer("grafana", "grafana/grafana", "12.0.2")
            .WithBindMount("Container/grafana/config", "/etc/grafana", true)
            .WithBindMount("Container/grafana/dashboards", "/var/lib/grafana/dashboards", true)
            .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint(Protocol.Http))
            .WithHttpEndpoint(targetPort: 3000, name: Protocol.Http);

        prometheus.WithParentRelationship(grafana);
        otel.WithReferenceRelationship(prometheus);

        return grafana;
    }

    /// <summary>
    ///     Sets the Grafana dashboard URL as an environment variable for the specified project resource.
    /// </summary>
    /// <param name="builder">The project resource builder to configure.</param>
    /// <param name="grafana">The Grafana container resource builder.</param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{ProjectResource}" /> with the Grafana URL environment variable set.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithGrafanaDashboard(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ContainerResource> grafana
    )
    {
        builder.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint(Protocol.Http));

        return builder;
    }

    private static IResourceBuilder<ContainerResource> AddPrometheus(
        this IDistributedApplicationBuilder builder
    )
    {
        var prometheus = builder
            .AddContainer("prometheus", "prom/prometheus", "v3.4.2")
            .WithBindMount("Container/prometheus", "/etc/prometheus", true)
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

        if (
            builder.IsHttpsLaunchProfile()
            && builder.ExecutionContext.IsRunMode
            && builder.Environment.IsDevelopment()
        )
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
