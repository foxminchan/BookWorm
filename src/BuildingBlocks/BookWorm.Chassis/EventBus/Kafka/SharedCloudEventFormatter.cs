using CloudNative.CloudEvents.SystemTextJson;
using MassTransit.Serialization;

namespace BookWorm.Chassis.EventBus.Kafka;

internal static class SharedCloudEventFormatter
{
    internal static readonly JsonEventFormatter Instance = new(
        SystemTextJsonMessageSerializer.Options,
        new()
    );
}
