using System.Reflection;
using System.Text.Json;
using BookWorm.Chassis.EventBus.Reflection;
using BookWorm.Constants.Aspire;
using JasperFx.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Configuration;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.HealthChecks;
using Wolverine.Kafka;
using Wolverine.Postgresql;

namespace BookWorm.Chassis.EventBus.Wolverine;

public static class WolverineHostExtensions
{
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
            var services = builder.Services;
            var kafkaConnectionString = builder.Configuration.GetConnectionString(
                Components.Broker
            );

            if (string.IsNullOrWhiteSpace(kafkaConnectionString))
            {
                return;
            }

            services.AddSingleton<UserIdEnvelopeMiddleware>();

            var applicationName = builder.Environment.ApplicationName;

            services.AddWolverine(ExtensionDiscovery.ManualOnly, opts =>
            {
                // ── Durable outbox/inbox ──────────────────────────────────────────
                opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                opts.Policies.UseDurableInboxOnAllListeners();
                opts.Durability.KeepAfterMessageHandling = TimeSpan.FromHours(1);
                opts.Durability.DeadLetterQueueExpirationEnabled = true;
                opts.Durability.DeadLetterQueueExpiration = TimeSpan.FromDays(7);

                // Ensures the Wolverine schema (wolverine_incoming, wolverine_outgoing, etc.)
                // is created on first boot without requiring a manual migration step (T040).
                opts.Services.AddResourceSetupOnStartup();

                // Log the start of every message execution at Debug level so OTel/structured
                // logs capture message flow without flooding production logs (T045).
                opts.Policies.LogMessageStarting(LogLevel.Debug);

                // ── Envelope rules (BookWorm header propagation) ───────────────────
                var sourceUrn = $"urn:bookworm:{applicationName.ToKebabCase()}";
                opts.MetadataRules.Add(new CloudEventHeaderPolicy(sourceUrn));
                opts.Policies.PropagateIncomingHeadersToOutgoing("requestid");

                // UserIdEnvelopeMiddleware stamps the HTTP user ID into envelope headers.
                // Resolves from DI so the shared IHttpContextAccessor instance is used.
                opts.Services.AddSingleton<IEnvelopeRule, UserIdEnvelopeMiddleware>(sp =>
                    sp.GetRequiredService<UserIdEnvelopeMiddleware>()
                );

                // ── Resilient error handling ──────────────────────────────────────
                opts.ConfigureDefaultErrorHandling();

                // ── Kafka transport with CloudEvents interop ──────────────────────
                opts.UseKafkaWithCloudEvents(
                    kafkaConnectionString,
                    applicationName,
                    builder.Environment.EnvironmentName
                );

                // ── Service-specific customisation ─────────────────────────────────
                configure?.Invoke(opts);
            });

            services
                .AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("Confluent.Kafka"))
                .WithMetrics(metrics => metrics.AddMeter("Wolverine"));

            services
                .AddHealthChecks()
                .AddWolverine(tags: ["live", "ready"])
                .AddWolverineListeners(tags: ["ready"]);
        }
    }

    extension(WolverineOptions opts)
    {
        private void ConfigureDefaultErrorHandling()
        {
            opts.OnException<TimeoutException>()
                .Or<HttpRequestException>()
                .Or<DbUpdateConcurrencyException>()
                .Or<NpgsqlException>(ex => ex.IsTransient)
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromSeconds(1)
                )
                .WithFullJitter()
                .Then.MoveToErrorQueue();

            opts.OnException<JsonException>().Or<FormatException>().MoveToErrorQueue();
        }

        /// <summary>
        ///     Configures PostgreSQL durable outbox/inbox storage and EF Core transactional
        ///     middleware when the specified connection string is present.
        /// </summary>
        /// <param name="configuration">Application configuration containing connection strings.</param>
        /// <param name="connectionName">Name of the PostgreSQL connection string.</param>
        public void UsePostgresOutbox(IConfiguration configuration, string connectionName)
        {
            var connectionString = configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            opts.PersistMessagesWithPostgresql(connectionString, "wolverine");
            opts.UseEntityFrameworkCoreTransactions();
        }

        private void UseKafkaWithCloudEvents(
            string kafkaConnectionString,
            string applicationName,
            string environmentName
        )
        {
            var serviceName = applicationName.ToKebabCase();
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
            var scannedTypes = assemblies.ScanIntegrationTypes();
            var messageTypes = scannedTypes.GetIntegrationEventTypes().ToArray();

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

            foreach (var messageType in scannedTypes.GetHandledIntegrationEventTypes().Distinct())
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
