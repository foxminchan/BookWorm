using System.Net.Mime;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Serialization;

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
