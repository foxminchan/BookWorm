using System.Net.Mime;
using CloudNative.CloudEvents;
using Confluent.Kafka;

namespace BookWorm.Chassis.EventBus.Kafka;

internal sealed class CloudEventKafkaSerializer<T> : IAsyncSerializer<T>
{
    public Task<byte[]> SerializeAsync(T data, SerializationContext context)
    {
        var cloudEvent = new CloudEvent
        {
            Id = Guid.CreateVersion7().ToString(),
            Type = typeof(T).FullName ?? typeof(T).Name,
            Source = new($"urn:masstransit:{context.Topic}"),
            Time = DateTimeOffset.UtcNow,
            DataContentType = MediaTypeNames.Application.Json,
            Data = data,
        };

        var bytes = SharedCloudEventFormatter
            .Instance.EncodeStructuredModeMessage(cloudEvent, out _)
            .ToArray();

        return Task.FromResult(bytes);
    }
}
