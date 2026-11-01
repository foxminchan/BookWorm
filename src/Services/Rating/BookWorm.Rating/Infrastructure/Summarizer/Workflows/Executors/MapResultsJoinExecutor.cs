namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

[SendsMessage(typeof(ReductionRequest))]
internal sealed class MapResultsJoinExecutor(int expectedWorkerCount)
    : Executor<MapShardResult>(nameof(MapResultsJoinExecutor))
{
    private readonly List<MapShardResult> _results = [];

    public override async ValueTask HandleAsync(
        MapShardResult message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        _results.Add(message);

        if (_results.Count < expectedWorkerCount)
        {
            return;
        }

        if (_results.Count > expectedWorkerCount)
        {
            throw new InvalidOperationException(
                "The map stage produced more results than expected."
            );
        }

        var summaries = _results
            .SelectMany(result => result.Summaries)
            .OrderBy(summary => summary.BatchIndex)
            .ToArray();

        if (summaries.Length == 0)
        {
            throw new InvalidOperationException("The map stage produced no partial summaries.");
        }

        var metrics = _results[0].Metrics;

        await context.SendMessageAsync(
            new ReductionRequest(summaries, metrics, 0, null),
            nameof(ReduceExecutor),
            cancellationToken
        );
    }
}
