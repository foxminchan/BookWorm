using System.Reflection;
using System.Text.Json;
using Wolverine;
using Wolverine.Kafka;

namespace BookWorm.Chassis.EventBus.Wolverine;

/// <summary>
///     Wolverine routing convention that maps every <see cref="IntegrationEvent" />
///     concrete type to and from its kebab-cased Kafka topic so topic names remain
///     stable and match the MassTransit-era <c>SetKebabCaseEndpointNameFormatter</c>
///     convention (FR-014, contracts/topic-routing.md).
/// </summary>
internal static class KafkaTopicRouter
{
    private static readonly JsonSerializerOptions _cloudEventJsonOptions = new(
        JsonSerializerDefaults.Web
    );

    /// <summary>
    ///     Registers publish and listen routing for every
    ///     <see cref="IntegrationEvent" /> type discovered in
    ///     <paramref name="assembly" />. Each type is routed to a kebab-cased topic
    ///     (e.g. <c>UserCheckedOutIntegrationEvent</c> →
    ///     <c>user-checked-out-integration-event</c>) and wired with CloudEvents
    ///     interop on both the sending and receiving sides.
    /// </summary>
    public static void RouteAllMessagesToKafkaByConvention(
        this WolverineOptions opts,
        Assembly assembly
    )
    {
        var integrationEventType = typeof(IntegrationEvent);

        var eventTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && integrationEventType.IsAssignableFrom(t));

        foreach (var eventType in eventTypes)
        {
            var topicName = ToKebabCase(eventType.Name);

            // Register a producer route so this service can publish the event to Kafka.
            opts.PublishMessage(eventType)
                .ToKafkaTopic(topicName)
                .InteropWithCloudEvents(_cloudEventJsonOptions);
        }
    }

    /// <summary>
    ///     Registers a Kafka listener for a specific integration event type on the
    ///     <c>bookworm.{serviceName}</c> consumer group.
    /// </summary>
    public static void ListenToKafkaTopicForEvent(
        this WolverineOptions opts,
        Type eventType,
        string serviceName
    )
    {
        var topicName = ToKebabCase(eventType.Name);
        var consumerGroup = $"bookworm.{ToKebabCase(serviceName)}";

        opts.ListenToKafkaTopic(topicName)
            .InteropWithCloudEvents(_cloudEventJsonOptions)
            .ConfigureConsumer(c => c.GroupId = consumerGroup)
            // Enable native Kafka dead-letter queue so CloudEvents whose `type` maps to no
            // known handler are dead-lettered with diagnostics rather than silently dropped
            // (spec.md § Edge Cases, T031).
            .EnableNativeDeadLetterQueue()
            .UseDurableInbox();
    }

    /// <summary>
    ///     Converts a PascalCase type name to its kebab-case equivalent.
    ///     <c>UserCheckedOutIntegrationEvent</c> → <c>user-checked-out-integration-event</c>
    /// </summary>
    public static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return string.Concat(
            name.Select(
                (c, i) =>
                    i > 0 && char.IsUpper(c)
                        ? "-" + char.ToLowerInvariant(c)
                        : char.ToLowerInvariant(c).ToString()
            )
        );
    }
}
