using BookWorm.Chassis.OpenTelemetry.ActivityScope;

namespace BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

public static class TelemetryTags
{
    public const string ActivityName = nameof(Marten);
    public const string Stream = $"{ActivitySourceProvider.DefaultSourceName}.stream";
}
