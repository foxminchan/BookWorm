using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Chassis.EventBus.Kafka;
using BookWorm.Contracts;
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
        var @event = CloudEventsMessageParser.Deserialize(
            message,
            CatalogIngestionSerializationContext.Default.ProcessVectorBatchIntegrationEvent
        );

        LogReceivedEvent(logger, @event.Id, message.Length);

        await processor.ProcessAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Received vector batch trigger event {EventId} ({Length} chars)"
    )]
    private static partial void LogReceivedEvent(ILogger logger, Guid eventId, int length);
}

[JsonSerializable(typeof(ProcessVectorBatchIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class CatalogIngestionSerializationContext : JsonSerializerContext;
