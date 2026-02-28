using System.Diagnostics;
using BookWorm.Chassis.Logging;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BookWorm.ServiceDefaults;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private static void AddLogging(this IHostApplicationBuilder builder)
    {
        var logger = builder.Logging;

        logger.EnableEnrichment();
        builder.Services.AddLogEnricher<ApplicationEnricher>();

        logger.AddGlobalBuffer(builder.Configuration.GetSection("Logging"));
        logger.AddPerIncomingRequestBuffer(builder.Configuration.GetSection("Logging"));

        logger.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        if (builder.Environment.IsDevelopment())
        {
            logger.AddTraceBasedSampler();
        }
    }

    private static void AddOpenTelemetry(
        this IServiceCollection services,
        IHostApplicationBuilder builder
    )
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ActivitySourceProvider.DefaultSourceName);
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        // Don't trace requests to the health endpoint to avoid filling the dashboard with noise
                        options.Filter = httpContext =>
                            !(
                                httpContext.Request.Path.StartsWithSegments(
                                    Http.Endpoints.HealthEndpointPath
                                )
                                || httpContext.Request.Path.StartsWithSegments(
                                    Http.Endpoints.AlivenessEndpointPath
                                )
                            )
                    )
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessor(new FixHttpRouteProcessor())
                    .AddSource(ActivitySourceProvider.DefaultSourceName);
            });
    }

    private static void AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks(Http.Endpoints.HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks(
            Http.Endpoints.AlivenessEndpointPath,
            new() { Predicate = r => r.Tags.Contains("live") }
        );
    }

    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        public void AddServiceDefaults()
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.RemoveAllResilienceHandlers();

                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });
        }

        private void ConfigureOpenTelemetry()
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();

            builder.AddLogging();

            services.AddOpenTelemetry(builder);

            builder.AddOpenTelemetryExporters();
        }

        private void AddOpenTelemetryExporters()
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(
                builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
            );

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }
        }
    }
}
