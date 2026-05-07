using System.Text;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Runtime.Interop;

namespace BookWorm.Chassis.EventBus.Wolverine;

internal sealed class CloudEventsOnlyKafkaMapper(CloudEventsMapper cloudEvents)
    : IKafkaEnvelopeMapper
{
    private const string TraceParentHeader = "traceparent";

    public void MapEnvelopeToOutgoing(Envelope envelope, Message<string, byte[]> outgoing)
    {
        if (!string.IsNullOrEmpty(envelope.GroupId))
        {
            outgoing.Key = envelope.GroupId;
        }

        outgoing.Value = cloudEvents.WriteToBytes(envelope);
        outgoing.Headers ??= [];

        if (!string.IsNullOrEmpty(envelope.ParentId))
        {
            outgoing.Headers.Add(TraceParentHeader, Encoding.UTF8.GetBytes(envelope.ParentId));
        }
    }

    public void MapIncomingToEnvelope(Envelope envelope, Message<string, byte[]> incoming)
    {
        envelope.Data = incoming.Value;

        var json = Encoding.UTF8.GetString(incoming.Value);
        cloudEvents.MapIncoming(envelope, json);
        envelope.Serializer = cloudEvents;

        var traceParent = TryReadHeader(incoming, TraceParentHeader);
        if (string.IsNullOrEmpty(traceParent))
        {
            traceParent = TryReadJsonString(json, TraceParentHeader);
        }

        if (!string.IsNullOrEmpty(traceParent))
        {
            envelope.ParentId = traceParent;
        }
    }

    private static string? TryReadHeader(Message<string, byte[]> message, string key)
    {
        return message.Headers is null
            ? null
            : (
                from header in message.Headers
                where string.Equals(header.Key, key, StringComparison.OrdinalIgnoreCase)
                select header.GetValueBytes() into value
                select value is null ? null : Encoding.UTF8.GetString(value)
            ).FirstOrDefault();
    }

    private static string? TryReadJsonString(string json, string property)
    {
        try
        {
            var node = JsonNode.Parse(json);
            return node?[property]?.GetValue<string>();
        }
        catch
        {
            return null;
        }
    }
}
