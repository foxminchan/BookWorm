using BookWorm.Ordering.Infrastructure.EventStore.Configs;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;
using JasperFx.CodeGeneration;
using Marten.Events.Daemon;

namespace BookWorm.Ordering.Infrastructure.EventStore.Extensions;

public static class Extensions
{
    public static void AddEventStore(
        this IHostApplicationBuilder builder,
        Action<StoreOptions>? configureOptions = null,
        bool disableAsyncDaemon = false
    )
    {
        var martenConfig = builder.Configuration.Get<MartenConfigs>() ?? new MartenConfigs();

        builder.AddNpgsqlDataSource(Components.Database.Ordering);

        var config = builder
            .Services.AddMarten(_ => StoreConfigs.SetStoreOptions(martenConfig, configureOptions))
            .UseNpgsqlDataSource()
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup();

        if (!disableAsyncDaemon)
        {
            config
                .OptimizeArtifactWorkflow(TypeLoadMode.Static)
                .AddAsyncDaemon(martenConfig.DaemonMode);
        }

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(t =>
                t.AddMeter(TelemetryTags.ActivityName, ActivitySourceProvider.DefaultSourceName)
            )
            .WithTracing(t =>
                t.AddSource(TelemetryTags.ActivityName, ActivitySourceProvider.DefaultSourceName)
            );

        builder
            .Services.AddHealthChecks()
            .AddMartenAsyncDaemonHealthCheck(500, TimeSpan.FromSeconds(30));
    }
}
