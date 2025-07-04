using BookWorm.Ordering.Infrastructure.EventStore.Configs;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;
using BookWorm.Ordering.Infrastructure.EventStore.Subscriptions;
using JasperFx.CodeGeneration;
using Marten.Events.Daemon;

namespace BookWorm.Ordering.Infrastructure.EventStore.Extensions;

internal static class Extensions
{
    public static void AddEventStore(
        this IHostApplicationBuilder builder,
        Action<StoreOptions>? configureStore = null,
        Action<MartenServiceCollectionExtensions.MartenConfigurationExpression>? configureMarten =
            null,
        bool disableAsyncDaemon = false
    )
    {
        var services = builder.Services;
        var martenConfig = builder.Configuration.Get<MartenConfigs>() ?? new MartenConfigs();

        builder.AddNpgsqlDataSource(Components.Database.Ordering);

        var config = services
            .AddMarten(_ => StoreConfigs.SetStoreOptions(martenConfig, configureStore))
            .UseNpgsqlDataSource()
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup();

        if (!disableAsyncDaemon)
        {
            config
                .AddAsyncDaemon(martenConfig.DaemonMode)
                .AddSubscriptionWithServices<MartenEventPublisher>(ServiceLifetime.Scoped);
        }

        configureMarten?.Invoke(config);

        // In a "Production" environment, we're turning off the
        // automatic database migrations and dynamic code generation
        services.CritterStackDefaults(x =>
        {
            x.Production.GeneratedCodeMode = TypeLoadMode.Static;
            x.Production.ResourceAutoCreate = AutoCreate.None;
        });

        services
            .AddOpenTelemetry()
            .WithMetrics(t =>
                t.AddMeter(TelemetryTags.ActivityName, ActivitySourceProvider.DefaultSourceName)
            )
            .WithTracing(t =>
                t.AddSource(TelemetryTags.ActivityName, ActivitySourceProvider.DefaultSourceName)
            );

        services.AddHealthChecks().AddMartenAsyncDaemonHealthCheck(500, TimeSpan.FromSeconds(30));
    }
}
