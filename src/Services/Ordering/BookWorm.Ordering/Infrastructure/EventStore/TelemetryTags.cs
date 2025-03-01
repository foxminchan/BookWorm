namespace BookWorm.Ordering.Infrastructure.EventStore;

public static class TelemetryTags
{
    public const string ActivityName = nameof(Marten);
    public const string Stream = $"{ActivitySourceProvider.DefaultSourceName}.stream";
}
