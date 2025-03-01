using Marten.Events.Daemon.Resiliency;

namespace BookWorm.Ordering.Infrastructure.EventStore.Configs;

public sealed class MartenConfigs
{
    public const string DefaultSchema = "public";
    public bool UseMetadata = true;
    public string WriteModelSchema { get; init; } = DefaultSchema;
    public string ReadModelSchema { get; init; } = DefaultSchema;
    public DaemonMode DaemonMode { get; init; } = DaemonMode.Solo;
}
