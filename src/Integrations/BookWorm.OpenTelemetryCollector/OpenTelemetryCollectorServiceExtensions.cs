using Aspire.Hosting.Lifecycle;

namespace BookWorm.OpenTelemetryCollector;

internal static class OpenTelemetryCollectorServiceExtensions
{
    public static void AddOpenTelemetryCollectorInfrastructure(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Services.TryAddLifecycleHook<OpenTelemetryCollectorLifecycleHook>();
    }
}
