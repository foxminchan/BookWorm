using System.Net.Mime;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;
using MassTransit.Serialization;

namespace BookWorm.Chassis.EventBus.Serialization;

/// <summary>
///     Factory that provides CloudEvents-based message serializer and deserializer
///     instances to MassTransit.
/// </summary>
/// <remarks>
///     The <see cref="JsonEventFormatter" /> is created with MassTransit's configured
///     <see cref="SystemTextJsonMessageSerializer.Options" /> so that message data
///     serialization is consistent with the rest of the bus pipeline.
/// </remarks>
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
