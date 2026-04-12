using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;
using MassTransit.Serialization;

namespace BookWorm.Chassis.EventBus.Serialization;

internal sealed class CloudEventMessageDeserializer(JsonEventFormatter formatter)
    : IMessageDeserializer
{
    public ContentType ContentType => CloudEventMessageSerializer.CloudEventContentType;

    public void Probe(ProbeContext context)
    {
        var scope = context.CreateScope("cloudevents-json");
        scope.Add("contentType", ContentType.MediaType);
        scope.Add("provider", "CloudNative.CloudEvents.SystemTextJson");
    }

    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        return new BodyConsumeContext(
            receiveContext,
            Deserialize(
                receiveContext.Body,
                receiveContext.TransportHeaders,
                receiveContext.InputAddress
            )
        );
    }

    public SerializerContext Deserialize(
        MessageBody body,
        Headers headers,
        Uri? destinationAddress = null
    )
    {
        try
        {
            var cloudEvent = formatter.DecodeStructuredModeMessage(
                body.GetBytes(),
                new(CloudEventMessageSerializer.CloudEventContentType.MediaType),
                CloudEventExtensions.All
            );

            var envelope = ToEnvelope(cloudEvent);

            var envelopeJson = JsonSerializer.SerializeToElement(
                envelope,
                SystemTextJsonMessageSerializer.Options
            );

            var reconstructedBody = new StringMessageBody(envelopeJson.GetRawText());

            return SystemTextJsonMessageSerializer.Instance.Deserialize(
                reconstructedBody,
                headers,
                destinationAddress
            );
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException(
                "An error occurred while deserializing the CloudEvent message envelope",
                ex
            );
        }
    }

    public MessageBody GetMessageBody(string text)
    {
        return new StringMessageBody(text);
    }

    private static JsonMessageEnvelope ToEnvelope(CloudEvent cloudEvent)
    {
        var messageId = cloudEvent.Id;
        var sentTime = cloudEvent.Time?.UtcDateTime;

        var correlationId = GetExtension(cloudEvent, CloudEventExtensions.CorrelationId);
        var conversationId = GetExtension(cloudEvent, CloudEventExtensions.ConversationId);
        var initiatorId = GetExtension(cloudEvent, CloudEventExtensions.InitiatorId);
        var requestId = GetExtension(cloudEvent, CloudEventExtensions.RequestId);
        var destinationAddress = GetExtension(cloudEvent, CloudEventExtensions.DestinationAddress);
        var responseAddress = GetExtension(cloudEvent, CloudEventExtensions.ResponseAddress);
        var faultAddress = GetExtension(cloudEvent, CloudEventExtensions.FaultAddress);
        var messageTypeHeader = GetExtension(cloudEvent, CloudEventExtensions.MessageType);

        var messageTypes = string.IsNullOrEmpty(messageTypeHeader)
            ? []
            : messageTypeHeader.Split(';', StringSplitOptions.RemoveEmptyEntries);

        var sourceAddress = cloudEvent.Source?.ToString();

        return new()
        {
            MessageId = messageId,
            CorrelationId = correlationId,
            ConversationId = conversationId,
            InitiatorId = initiatorId,
            RequestId = requestId,
            SourceAddress = sourceAddress,
            DestinationAddress = destinationAddress,
            ResponseAddress = responseAddress,
            FaultAddress = faultAddress,
            SentTime = sentTime,
            MessageType = messageTypes,
            Message = cloudEvent.Data,
        };
    }

    private static string? GetExtension(CloudEvent cloudEvent, CloudEventAttribute attribute)
    {
        return cloudEvent[attribute] as string;
    }
}
