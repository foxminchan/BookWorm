using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace BookWorm.Chassis.EventBus.Kafka;

public static class CloudEventsMessageParser
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public static T Deserialize<T>(string rawMessage)
        where T : IntegrationEvent
    {
        var payloadElement = ExtractPayloadElement(rawMessage);

        return payloadElement.Deserialize<T>(_jsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize CloudEvents payload as '{typeof(T).Name}'."
            );
    }

    public static T Deserialize<T>(string rawMessage, JsonTypeInfo<T> jsonTypeInfo)
        where T : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var payloadElement = ExtractPayloadElement(rawMessage);

        return payloadElement.Deserialize(jsonTypeInfo)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize CloudEvents payload as '{typeof(T).Name}'."
            );
    }

    private static JsonElement ExtractPayloadElement(string rawMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawMessage);

        using var document = JsonDocument.Parse(rawMessage);
        var payloadElement = document.RootElement.TryGetProperty("data", out var dataElement)
            ? dataElement
            : document.RootElement;

        return payloadElement.Clone();
    }
}
