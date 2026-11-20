using System.Diagnostics;

namespace BookWorm.Chassis.OpenTelemetry.ActivityScope;

public static class ActivitySourceProvider
{
    public const string DefaultSourceName = nameof(BookWorm);

    public static readonly ActivitySource Instance = new(DefaultSourceName, "v1");
}
