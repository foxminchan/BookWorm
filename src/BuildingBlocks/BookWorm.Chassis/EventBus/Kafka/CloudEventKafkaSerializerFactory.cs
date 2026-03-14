using System.Net.Mime;
using Confluent.Kafka;
using MassTransit.KafkaIntegration.Serializers;

namespace BookWorm.Chassis.EventBus.Kafka;

/// <summary>
///     Kafka-level serialization factory that wraps message values in
///     <see href="https://cloudevents.io/">CloudEvents</see> structured-content JSON envelopes.
/// </summary>
/// <remarks>
///     Plugged into the Kafka rider via
///     <see cref="MassTransit.IKafkaFactoryConfigurator.SetSerializationFactory" />,
///     replacing the default <see cref="DefaultKafkaSerializerFactory" />.
///     Primitive Kafka key types (<see cref="Null" />, <c>byte[]</c>, <c>string</c>)
///     delegate to their built-in Confluent serializers.
/// </remarks>
internal sealed class CloudEventKafkaSerializerFactory : IKafkaSerializerFactory
{
    public ContentType ContentType => new("application/cloudevents+json");

    public IAsyncSerializer<T> GetSerializer<T>()
    {
        if (typeof(T) == typeof(Null))
        {
            return (IAsyncSerializer<T>)(object)new NullAsyncSerializer();
        }

        if (typeof(T) == typeof(byte[]))
        {
            return (IAsyncSerializer<T>)(object)new ByteArrayAsyncSerializer();
        }

        if (typeof(T) == typeof(string))
        {
            return (IAsyncSerializer<T>)(object)new StringAsyncSerializer();
        }

        return new CloudEventKafkaSerializer<T>();
    }

    public IDeserializer<T> GetDeserializer<T>()
    {
        if (typeof(T) == typeof(Null))
        {
            return (IDeserializer<T>)Deserializers.Null;
        }

        if (typeof(T) == typeof(byte[]))
        {
            return (IDeserializer<T>)Deserializers.ByteArray;
        }

        if (typeof(T) == typeof(string))
        {
            return (IDeserializer<T>)Deserializers.Utf8;
        }

        return new CloudEventKafkaDeserializer<T>();
    }

    private sealed class NullAsyncSerializer : IAsyncSerializer<Null>
    {
        public Task<byte[]> SerializeAsync(Null data, SerializationContext context)
        {
            return Task.FromResult(Serializers.Null.Serialize(data, context));
        }
    }

    private sealed class ByteArrayAsyncSerializer : IAsyncSerializer<byte[]>
    {
        public Task<byte[]> SerializeAsync(byte[] data, SerializationContext context)
        {
            return Task.FromResult(Serializers.ByteArray.Serialize(data, context));
        }
    }

    private sealed class StringAsyncSerializer : IAsyncSerializer<string>
    {
        public Task<byte[]> SerializeAsync(string data, SerializationContext context)
        {
            return Task.FromResult(Serializers.Utf8.Serialize(data, context));
        }
    }
}
