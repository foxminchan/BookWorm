using System.Reflection;
using System.Text.Json;
using BookWorm.Constants.Aspire;
using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scrutor;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Configuration;
using Wolverine.Kafka;

namespace BookWorm.Chassis.EventBus.Wolverine;

public static class WolverineHostExtensions
{
    private static Type[] ScanIntegrationTypes(IEnumerable<Assembly> assemblies)
    {
        var scannedServices = new ServiceCollection();

        scannedServices.Scan(scan =>
            scan.FromAssemblies(assemblies.ToArray())
                .AddClasses(
                    classes => classes.Where(IsIntegrationMessageOrHandler),
                    publicOnly: false
                )
                .AsSelf()
                .WithTransientLifetime()
        );

        return scannedServices
            .Select(descriptor => descriptor.ImplementationType)
            .Where(type => type is not null)
            .Cast<Type>()
            .Distinct()
            .ToArray();
    }

    private static bool IsIntegrationMessageOrHandler(Type type)
    {
        return typeof(IntegrationEvent).IsAssignableFrom(type)
            || type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Any(method =>
                    IsMessageHandlerMethod(method)
                    && typeof(IntegrationEvent).IsAssignableFrom(
                        method.GetParameters()[0].ParameterType
                    )
                );
    }

    private static IEnumerable<Type> GetHandledIntegrationEventTypes(IEnumerable<Type> types)
    {
        return types
            .Where(t => !typeof(IntegrationEvent).IsAssignableFrom(t))
            .SelectMany(t =>
                t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            )
            .Where(IsMessageHandlerMethod)
            .Select(m => m.GetParameters()[0].ParameterType)
            .Where(t => typeof(IntegrationEvent).IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetIntegrationEventTypes(IEnumerable<Type> types)
    {
        return types.Where(t => typeof(IntegrationEvent).IsAssignableFrom(t));
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
                    opts.UseKafkaWithCloudEvents(
                        kafkaConnectionString,
                        applicationName,
                        builder.Environment.EnvironmentName
                    );

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
        private void UseKafkaWithCloudEvents(
            string kafkaConnectionString,
            string applicationName,
            string environmentName
        )
        {
            var serviceName = KafkaTopicRouter.ToKebabCase(applicationName);
            opts.ServiceName = serviceName;

            var cloudEventsJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            // ── Native Kafka dead letter queue ────────────────────────────────
            opts.UseKafka(kafkaConnectionString)
                .AutoProvision()
                .DeadLetterQueueTopicName(
                    KafkaTopicRouter.CreateDeadLetterTopicName(applicationName, environmentName)
                );

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
        }

        /// <summary>
        ///     Scans the specified assemblies for integration messages and handlers, configures
        ///     publishing routes, and registers listeners for handled messages.
        /// </summary>
        /// <param name="environment">Host environment used to construct Kafka topic names.</param>
        /// <param name="assemblies">Assemblies containing integration messages and handlers.</param>
        public void ConfigureIntegrationEventTopicsIn(
            IHostEnvironment environment,
            params Assembly[] assemblies
        )
        {
            var scannedTypes = ScanIntegrationTypes(assemblies);
            var messageTypes = GetIntegrationEventTypes(scannedTypes).ToArray();

            foreach (var messageType in messageTypes)
            {
                var identity = messageType.GetCustomAttribute<MessageIdentityAttribute>();
                var messageIdentity = identity?.Alias ?? messageType.FullName ?? messageType.Name;
                var topic = KafkaTopicRouter.CreateTopicName(
                    messageType,
                    environment.ApplicationName,
                    environment.EnvironmentName
                );
                opts.RegisterMessageType(messageType, messageIdentity);
                opts.PublishMessage(messageType).ToKafkaTopic(topic).TelemetryEnabled(true);
            }

            foreach (var messageType in GetHandledIntegrationEventTypes(scannedTypes).Distinct())
            {
                var topic = KafkaTopicRouter.CreateTopicName(
                    messageType,
                    environment.ApplicationName,
                    environment.EnvironmentName
                );
                // Route any message that exhausts its retry policy to the transport-level
                // native dead letter queue topic configured in `UseKafkaWithCloudEvents`.
                opts.ListenToKafkaTopic(topic).EnableNativeDeadLetterQueue();
            }
        }
    }
}
