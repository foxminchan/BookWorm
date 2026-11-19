using System.Text.Json;

namespace BookWorm.Chassis.EventBus.Kafka;

public static class CloudEventsMessageParser
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public static T Deserialize<T>(string rawMessage)
        where T : IntegrationEvent
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawMessage);

        using var document = JsonDocument.Parse(rawMessage);
        var payloadElement = document.RootElement.TryGetProperty("data", out var dataElement)
            ? dataElement
            : document.RootElement;

        return payloadElement.Deserialize<T>(_jsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize CloudEvents payload as '{typeof(T).Name}'."
            );
    }
}
