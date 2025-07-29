using System.Diagnostics;
using BookWorm.Chassis.Logging;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.ServiceDefaults.ApiSpecification;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BookWorm.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthChecks = nameof(HealthChecks);
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static void AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
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

    private static void ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        builder.AddLogging();

        services.AddOpenTelemetry(builder);

        builder.AddOpenTelemetryExporters();
    }

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
                                httpContext.Request.Path.StartsWithSegments(HealthEndpointPath)
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

    private static void AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        );

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
    }

    private static void AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var healthChecksConfiguration = builder.Configuration.GetSection(HealthChecks);

        // All health checks endpoints must return within the configured timeout value (defaults to 5 seconds)
        var healthChecksRequestTimeout =
            healthChecksConfiguration.GetValue<TimeSpan?>("RequestTimeout")
            ?? TimeSpan.FromSeconds(5);

        services.AddRequestTimeouts(timeouts =>
            timeouts.AddPolicy(HealthChecks, healthChecksRequestTimeout)
        );

        // Cache health checks responses for the configured duration (defaults to 10 seconds)
        var healthChecksExpireAfter =
            healthChecksConfiguration.GetValue<TimeSpan?>("ExpireAfter")
            ?? TimeSpan.FromSeconds(10);

        services.AddOutputCache(caching =>
            caching.AddPolicy(HealthChecks, policy => policy.Expire(healthChecksExpireAfter))
        );

        services
            .AddHealthChecks()
            // Add a default liveness check to ensure the app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        // Configure the health checks
        var healthChecks = app.MapGroup("");

        // Configure health checks endpoints to use the configured request timeouts and cache policies
        healthChecks.CacheOutput(HealthChecks).WithRequestTimeout(HealthChecks);

        // All health checks must pass for the app to be considered ready to accept traffic after starting
        healthChecks.MapHealthChecks(HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for the app to be considered alive
        healthChecks.MapHealthChecks(
            AlivenessEndpointPath,
            new() { Predicate = r => r.Tags.Contains("live") }
        );

        // Add the health checks endpoint for the HealthChecksUI
        var healthChecksUrls = app.Configuration["HEALTHCHECKSUI_URLS"];
        if (string.IsNullOrWhiteSpace(healthChecksUrls))
        {
            return;
        }

        var pathToHostsMap = GetPathToHostsMap(healthChecksUrls);

        foreach (var path in pathToHostsMap.Keys)
        {
            // Ensure that the HealthChecksUI endpoint is only accessible from configured hosts,
            // e.g., localhost:12345, hub.docker.internal, etc.
            // as it contains more detailed information about the health of the app,
            // including the types of dependencies it has.
            healthChecks
                .MapHealthChecks(
                    path,
                    new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
                )
                // This ensures that the HealthChecksUI endpoint is only accessible from the configured health checks URLs.
                // See this documentation to learn more about restricting access to health checks endpoints via routing:
                // https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0#use-health-checks-routing
                .RequireHost(pathToHostsMap[path]);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect(HealthEndpointPath)).ExcludeFromDescription();
        }
    }

    private static Dictionary<string, string[]> GetPathToHostsMap(string healthChecksUrls)
    {
        // Given a value like "localhost:12345/healthz;hub.docker.internal:12345/healthz" return a dictionary like:
        // { { "healthz", [ "localhost:12345", "hub.docker.internal:12345" ] } }
        var uris = healthChecksUrls
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(url => new Uri(url, UriKind.Absolute))
            .GroupBy(uri => uri.AbsolutePath, uri => uri.Authority)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return uris;
    }
}
