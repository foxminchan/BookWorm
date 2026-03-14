using System.Text.Json;
using Confluent.Kafka;
using MassTransit.Serialization;

namespace BookWorm.Chassis.EventBus.Kafka;

internal sealed class CloudEventKafkaDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            return default!;
        }

        var cloudEvent = SharedCloudEventFormatter.Instance.DecodeStructuredModeMessage(
            data.ToArray(),
            new("application/cloudevents+json"),
            null
        );

        if (cloudEvent.Data is JsonElement jsonElement)
        {
            return jsonElement.Deserialize<T>(SystemTextJsonMessageSerializer.Options)!;
        }

        return (T)cloudEvent.Data!;
    }
}
