using System.Diagnostics;
using BookWorm.Chassis.Logging;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Chassis.Utilities.Configuration;
using BookWorm.ServiceDefaults.ApiSpecification;
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
    private const string AlivenessEndpointPath = "/alive";

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
                                    Restful.Host.HealthEndpointPath
                                )
                                || httpContext.Request.Path.StartsWithSegments(
                                    AlivenessEndpointPath
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
        app.MapHealthChecks(Restful.Host.HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks(
            AlivenessEndpointPath,
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

            builder.Services.Configure<DocumentOptions>(DocumentOptions.ConfigurationSection);

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler(options =>
                {
                    var timeSpan = TimeSpan.FromMinutes(2);
                    options.AttemptTimeout.Timeout = timeSpan;
                    options.CircuitBreaker.SamplingDuration = timeSpan * 2;
                    options.TotalRequestTimeout.Timeout = timeSpan * 3;
                    options.Retry.MaxRetryAttempts = 1;
                });

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
