namespace BookWorm.Ordering.Infrastructure.Marten;

public sealed class MartenConfigs
{
    public const string DefaultSchema = "order_state";
    public bool UseMetadata = true;
    public string WriteModelSchema { get; set; } = DefaultSchema;
    public string ReadModelSchema { get; set; } = DefaultSchema;
    public DaemonMode DaemonMode { get; set; } = DaemonMode.Solo;
}
