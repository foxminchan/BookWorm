using MassTransit;

namespace BookWorm.Chassis.EventBus.Serialization;

/// <summary>
///     Extension methods to configure CloudEvents serialization on MassTransit bus
///     and receive endpoint configurators.
/// </summary>
/// <remarks>
///     Follows the pattern established by
///     <see href="https://github.com/riezebosch/CloudEventify">CloudEventify</see>:
///     the deserializer is registered as the default content type handler so incoming
///     <c>application/cloudevents+json</c> messages are consumed automatically, while
///     the serializer wraps all outgoing messages in a CloudEvents envelope.
/// </remarks>
public static class CloudEventConfiguratorExtensions
{
    public static void UseCloudEvents(this IBusFactoryConfigurator configurator)
    {
        var factory = new CloudEventSerializerFactory();
        configurator.AddSerializer(factory);
        configurator.AddDeserializer(factory, true);
    }

    public static void UseCloudEvents(this IReceiveEndpointConfigurator configurator)
    {
        var factory = new CloudEventSerializerFactory();
        configurator.AddSerializer(factory);
        configurator.AddDeserializer(factory, true);
    }
}
