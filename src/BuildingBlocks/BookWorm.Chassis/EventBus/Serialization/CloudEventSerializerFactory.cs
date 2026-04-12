using System.Net.Mime;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;
using MassTransit.Serialization;

namespace BookWorm.Chassis.EventBus.Serialization;

internal sealed class CloudEventSerializerFactory : ISerializerFactory
{
    public ContentType ContentType => CloudEventMessageSerializer.CloudEventContentType;

    public IMessageSerializer CreateSerializer()
    {
        return new CloudEventMessageSerializer(CreateFormatter());
    }

    public IMessageDeserializer CreateDeserializer()
    {
        return new CloudEventMessageDeserializer(CreateFormatter());
    }

    private static JsonEventFormatter CreateFormatter()
    {
        return new(SystemTextJsonMessageSerializer.Options, new());
    }
}
