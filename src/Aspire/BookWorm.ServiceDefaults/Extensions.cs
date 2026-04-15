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
    extension(IHostApplicationBuilder builder)
    {
        private void AddDefaultHealthChecks()
        {
            builder
                .Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        ///     Maps development-only health check endpoints for readiness and liveness probing.
        /// </summary>
        /// <remarks>
        ///     The readiness endpoint requires all checks to pass, while the liveness endpoint
        ///     evaluates only checks tagged with <c>live</c>.
        /// </remarks>
        public void MapDefaultEndpoints()
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            // All health checks must pass for app to be considered ready to accept traffic after starting.
            app.MapHealthChecks(Http.Endpoints.HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive.
            app.MapHealthChecks(
                Http.Endpoints.AlivenessEndpointPath,
                new() { Predicate = r => r.Tags.Contains("live") }
            );
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        private void AddLogging()
        {
            var logger = builder.Logging;
            var loggerSection = builder.Configuration.GetSection("Logging");

            logger.EnableEnrichment();
            builder.AddApplicationEnricher();

            logger.AddGlobalBuffer(loggerSection);
            logger.AddPerIncomingRequestBuffer(loggerSection);

            logger.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            if (builder.Environment.IsDevelopment())
            {
                logger.AddTraceBasedSampler();
            }
            else
            {
                logger.AddRandomProbabilisticSampler(loggerSection);
            }
        }
    }

    extension(IServiceCollection services)
    {
        private void AddOpenTelemetry(IHostApplicationBuilder builder)
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
                        .AddFixHttpRouteProcessor()
                        .AddSource(ActivitySourceProvider.DefaultSourceName);
                });
        }
    }

    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        /// <summary>
        ///     Configures the default platform capabilities for a service host.
        /// </summary>
        /// <remarks>
        ///     This enables OpenTelemetry, baseline health checks, service discovery, and
        ///     default HTTP client resilience/service discovery behavior.
        /// </remarks>
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
