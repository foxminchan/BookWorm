namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

internal sealed class PartitionExecutor(int batchSize, int maxConcurrency)
    : Executor<RatingWorkflowInput, ReviewPartition>(nameof(PartitionExecutor))
{
    public override ValueTask<ReviewPartition> HandleAsync(
        RatingWorkflowInput message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        return ValueTask.FromResult(Partition(message, batchSize, maxConcurrency));
    }

    private static ReviewPartition Partition(
        RatingWorkflowInput input,
        int batchSize,
        int maxConcurrency
    )
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrency);

        if (input.Reviews.Length == 0)
        {
            throw new ArgumentException("At least one review is required.", nameof(input));
        }

        var batches = input
            .Reviews.OrderBy(review => review.Id)
            .Chunk(batchSize)
            .Select((reviews, index) => new ReviewBatch(index, reviews))
            .ToArray();

        var workerCount = Math.Min(maxConcurrency, batches.Length);
        var workerBatches = Enumerable
            .Range(0, workerCount)
            .Select(workerIndex =>
                batches.Where((_, batchIndex) => batchIndex % workerCount == workerIndex).ToArray()
            )
            .ToArray();

        return new(workerBatches, new(input.AverageRating, input.ReviewCount));
    }
}
