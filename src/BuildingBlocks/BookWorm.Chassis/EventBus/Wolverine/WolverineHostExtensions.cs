using System.Reflection;
using System.Text.Json;
using BookWorm.Constants.Aspire;
using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Configuration;
using Wolverine.Kafka;

namespace BookWorm.Chassis.EventBus.Wolverine;

public static class WolverineHostExtensions
{
    private static IEnumerable<Type> GetIntegrationEventTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t =>
                t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            )
            .Where(IsMessageHandlerMethod)
            .Select(m => m.GetParameters()[0].ParameterType)
            .Where(t => typeof(IntegrationEvent).IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).Cast<Type>();
        }
    }

    private static bool IsMessageHandlerMethod(MethodInfo method)
    {
        return method.Name is "Handle" or "HandleAsync" or "Consume" or "ConsumeAsync"
            && method.GetParameters().Length > 0;
    }

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers WolverineFx with per-service Postgres durable outbox/inbox,
        ///     CloudEvents-compatible Kafka transport, and BookWorm header propagation
        ///     policies. When no broker connection string is configured the call is a
        ///     no-op so services can start without a live Kafka cluster (FR-015).
        /// </summary>
        /// <param name="configure">
        ///     Optional callback for additional <see cref="WolverineOptions" />
        ///     customization (e.g. service-specific handler discovery or endpoint overrides).
        /// </param>
        public void UseEventFramework(Action<WolverineOptions>? configure = null)
        {
            var kafkaConnectionString = builder.Configuration.GetConnectionString(
                Components.Broker
            );

            if (string.IsNullOrWhiteSpace(kafkaConnectionString))
            {
                return;
            }

            builder.Services.AddSingleton<UserIdEnvelopeMiddleware>();

            var applicationName = builder.Environment.ApplicationName;

            builder.Services.AddWolverine(
                ExtensionDiscovery.ManualOnly,
                opts =>
                {
                    // ── Durable outbox/inbox ──────────────────────────────────────────
                    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                    opts.Policies.UseDurableInboxOnAllListeners();

                    // Ensures the Wolverine schema (wolverine_incoming, wolverine_outgoing, etc.)
                    // is created on first boot without requiring a manual migration step (T040).
                    opts.Services.AddResourceSetupOnStartup();

                    // Log the start of every message execution at Debug level so OTel/structured
                    // logs capture message flow without flooding production logs (T045).
                    opts.Policies.LogMessageStarting(LogLevel.Debug);

                    // ── Envelope rules (BookWorm header propagation) ───────────────────
                    var sourceUrn = $"urn:bookworm:{KafkaTopicRouter.ToKebabCase(applicationName)}";
                    opts.MetadataRules.Add(new CloudEventHeaderPolicy(sourceUrn));

                    // UserIdEnvelopeMiddleware stamps the HTTP user ID into envelope headers.
                    // Resolves from DI so the shared IHttpContextAccessor instance is used.
                    opts.Services.AddSingleton<IEnvelopeRule, UserIdEnvelopeMiddleware>(sp =>
                        sp.GetRequiredService<UserIdEnvelopeMiddleware>()
                    );

                    // ── Kafka transport with CloudEvents interop ──────────────────────
                    opts.UseKafkaWithCloudEvents(kafkaConnectionString, applicationName);

                    // ── Service-specific customisation ─────────────────────────────────
                    configure?.Invoke(opts);
                }
            );

            builder
                .Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("Confluent.Kafka"))
                .WithMetrics(metrics => metrics.AddMeter("Wolverine"));
        }
    }

    extension(WolverineOptions opts)
    {
        private void UseKafkaWithCloudEvents(string kafkaConnectionString, string applicationName)
        {
            var serviceName = KafkaTopicRouter.ToKebabCase(applicationName);
            opts.ServiceName = serviceName;

            var cloudEventsJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            // ── Native Kafka dead letter queue ────────────────────────────────
            opts.UseKafka(kafkaConnectionString)
                .AutoProvision()
                .DeadLetterQueueTopicName($"bookworm-{serviceName}-dead-letter-queue");

            opts.Policies.PropagateGroupIdToPartitionKey();

            opts.Policies.Add(
                new LambdaEndpointPolicy<KafkaTopic>(
                    (topic, runtime) =>
                    {
                        var cloudEvents = topic.BuildCloudEventsMapper(
                            runtime,
                            cloudEventsJsonOptions
                        );
                        topic.EnvelopeMapper = new CloudEventsOnlyKafkaMapper(cloudEvents);
                        topic.DefaultSerializer = cloudEvents;
                    }
                )
            );

            opts.PublishAllMessages().ToKafkaTopics().TelemetryEnabled(true);
        }

        /// <summary>
        ///     Scans the specified assemblies for public instance/static methods with a single parameter
        ///     of type <see cref="IntegrationEvent" /> and registers a Kafka listener for each
        ///     unique message type found. The listener will be configured to subscribe to the topic
        ///     specified in the <see cref="MessageIdentityAttribute" /> of the message type, or
        ///     the message type's full name if the attribute is not present.
        /// </summary>
        /// <param name="assemblies">List of assemblies to scan for message handler methods</param>
        public void ListenToIntegrationEventsIn(params Assembly[] assemblies)
        {
            var topics = new Dictionary<string, Type>(StringComparer.Ordinal);

            foreach (var messageType in GetIntegrationEventTypes(assemblies))
            {
                var identity = messageType.GetCustomAttribute<MessageIdentityAttribute>();
                var topic = identity?.Alias ?? messageType.FullName ?? messageType.Name;
                topics[topic] = messageType;
            }

            foreach (var (topic, messageType) in topics)
            {
                opts.RegisterMessageType(messageType, topic);

                // Route any message that exhausts its retry policy to the transport-level
                // native dead letter queue topic configured in `UseKafkaWithCloudEvents`.
                opts.ListenToKafkaTopic(topic).EnableNativeDeadLetterQueue();
            }
        }
    }
}
