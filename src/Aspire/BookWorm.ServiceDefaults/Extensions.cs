using System.Diagnostics;
using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.Logging;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.ServiceDefaults.ApiSpecification;
using BookWorm.ServiceDefaults.Kestrel;
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
            http.AddStandardResilienceHandler();

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
        var healthChecksConfiguration = builder.Configuration.GetSection(HealthChecks);

        // All health checks endpoints must return within the configured timeout value (defaults to 5 seconds)
        var healthChecksRequestTimeout =
            healthChecksConfiguration.GetValue<TimeSpan?>("RequestTimeout")
            ?? TimeSpan.FromSeconds(5);

        builder.Services.AddRequestTimeouts(timeouts =>
            timeouts.AddPolicy(HealthChecks, healthChecksRequestTimeout)
        );

        // Cache health checks responses for the configured duration (defaults to 10 seconds)
        var healthChecksExpireAfter =
            healthChecksConfiguration.GetValue<TimeSpan?>("ExpireAfter")
            ?? TimeSpan.FromSeconds(10);

        builder.Services.AddOutputCache(caching =>
            caching.AddPolicy(HealthChecks, policy => policy.Expire(healthChecksExpireAfter))
        );

        builder
            .Services.AddHealthChecks()
            // Add a default liveness check to ensure the app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days.
            // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseExceptionHandler();

        app.UseStatusCodePages();

        app.UseDefaultCors();

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

        // Since OpenAPI/AsyncAPI routes are only available in development mode,
        // we redirect the root path to the health endpoint in production
        if (!app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/health")).ExcludeFromDescription();
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
