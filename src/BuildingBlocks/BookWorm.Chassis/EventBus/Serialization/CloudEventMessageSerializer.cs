using System.Net.Mime;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Serialization;

/// <summary>
///     MassTransit message serializer that wraps outgoing messages in the
///     <see href="https://cloudevents.io/">CloudEvents</see> structured-content JSON envelope
///     using <c>CloudNative.CloudEvents.SystemTextJson</c>.
/// </summary>
internal sealed class CloudEventMessageSerializer(JsonEventFormatter formatter) : IMessageSerializer
{
    internal static readonly ContentType CloudEventContentType = new(
        "application/cloudevents+json"
    );

    public ContentType ContentType => CloudEventContentType;

    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class
    {
        return new CloudEventMessageBody<T>(context, formatter);
    }
}
