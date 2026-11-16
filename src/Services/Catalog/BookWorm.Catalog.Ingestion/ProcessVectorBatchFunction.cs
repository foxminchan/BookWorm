using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Catalog.Ingestion;

public sealed partial class ProcessVectorBatchFunction(
    IVectorBatchProcessor processor,
    ILogger<ProcessVectorBatchFunction> logger
)
{
    private const string TopicName = $"{Services.Scheduler}.process-vector-batch";

    [Function(nameof(ProcessVectorBatch))]
    public async Task ProcessVectorBatch(
        [KafkaTrigger("%BrokerList%", TopicName, ConsumerGroup = Services.CatalogIngestion)]
            string message,
        CancellationToken cancellationToken
    )
    {
        LogReceivedEvent(logger, message.Length);

        await processor.ProcessAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Received vector batch trigger event ({Length} chars)"
    )]
    private static partial void LogReceivedEvent(ILogger logger, int length);
}
