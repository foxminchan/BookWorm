using MassTransit;

namespace BookWorm.Chassis.EventBus.Serialization;

public static class CloudEventConfiguratorExtensions
{
    extension(IBusFactoryConfigurator configurator)
    {
        /// <summary>
        ///     Configures this MassTransit configurator to use CloudEvents for message serialization and deserialization.
        /// </summary>
        /// <remarks>
        ///     A single <see cref="CloudEventSerializerFactory" /> instance is registered for both serializer and deserializer
        ///     to ensure consistent payload handling.
        /// </remarks>
        public void UseCloudEvents()
        {
            var factory = new CloudEventSerializerFactory();
            configurator.AddSerializer(factory);
            configurator.AddDeserializer(factory, true);
        }
    }

    extension(IReceiveEndpointConfigurator configurator)
    {
        /// <summary>
        ///     Configures this receive endpoint to use CloudEvents for message serialization and deserialization.
        /// </summary>
        /// <remarks>
        ///     A single <see cref="CloudEventSerializerFactory" /> instance is used for both serializer and deserializer
        ///     to keep payload handling consistent for this endpoint.
        /// </remarks>
        public void UseCloudEvents()
        {
            var factory = new CloudEventSerializerFactory();
            configurator.AddSerializer(factory);
            configurator.AddDeserializer(factory, true);
        }
    }
}
