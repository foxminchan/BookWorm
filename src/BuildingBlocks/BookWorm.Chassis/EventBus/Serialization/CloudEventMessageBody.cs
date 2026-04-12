using System.Net.Mime;
using System.Text;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Serialization;

internal sealed class CloudEventMessageBody<T>(SendContext<T> context, JsonEventFormatter formatter)
    : MessageBody
    where T : class
{
    private byte[]? _bytes;

    public long? Length => _bytes?.Length;

    public Stream GetStream()
    {
        return new MemoryStream(GetBytes());
    }

    public byte[] GetBytes()
    {
        return _bytes ??= Serialize();
    }

    public string GetString()
    {
        return Encoding.UTF8.GetString(GetBytes());
    }

    private byte[] Serialize()
    {
        var messageTypes = context.SupportedMessageTypes;

        var cloudEvent = new CloudEvent
        {
            Id = (context.MessageId ?? NewId.NextGuid()).ToString(),
            Type = CloudEventTypes.FromMessageTypes(messageTypes),
            Source = context.SourceAddress ?? new Uri("urn:masstransit:unknown"),
            Time = context.SentTime,
            DataContentType = MediaTypeNames.Application.Json,
            Data = context.Message,
        };

        if (context.CorrelationId.HasValue)
        {
            cloudEvent[CloudEventExtensions.CorrelationId] = context.CorrelationId.Value.ToString();
        }

        if (context.ConversationId.HasValue)
        {
            cloudEvent[CloudEventExtensions.ConversationId] =
                context.ConversationId.Value.ToString();
        }

        if (context.InitiatorId.HasValue)
        {
            cloudEvent[CloudEventExtensions.InitiatorId] = context.InitiatorId.Value.ToString();
        }

        if (context.RequestId.HasValue)
        {
            cloudEvent[CloudEventExtensions.RequestId] = context.RequestId.Value.ToString();
        }

        if (context.DestinationAddress is not null)
        {
            cloudEvent[CloudEventExtensions.DestinationAddress] =
                context.DestinationAddress.ToString();
        }

        if (context.ResponseAddress is not null)
        {
            cloudEvent[CloudEventExtensions.ResponseAddress] = context.ResponseAddress.ToString();
        }

        if (context.FaultAddress is not null)
        {
            cloudEvent[CloudEventExtensions.FaultAddress] = context.FaultAddress.ToString();
        }

        if (messageTypes.Length > 0)
        {
            cloudEvent[CloudEventExtensions.MessageType] = string.Join(";", messageTypes);
        }

        return formatter.EncodeStructuredModeMessage(cloudEvent, out _).ToArray();
    }
}
