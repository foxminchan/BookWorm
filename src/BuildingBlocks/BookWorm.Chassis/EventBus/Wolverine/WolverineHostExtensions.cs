using BookWorm.Constants.Aspire;
using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Kafka;

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
            var kafkaConnectionString = builder.Configuration.GetConnectionString(
                Components.Broker
            );

            if (string.IsNullOrWhiteSpace(kafkaConnectionString))
            {
                return;
            }

            // IHttpContextAccessor is already registered by ServiceDefaults.ConfigureOpenTelemetry;
            // AddHttpContextAccessor uses TryAddSingleton, but we omit the redundant call here.
            builder.Services.AddSingleton<UserIdEnvelopeMiddleware>();

            var applicationName = builder.Environment.ApplicationName;

            builder.UseWolverine(opts =>
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
                // CloudEventHeaderPolicy stamps messagetype, destinationaddress, and
                // responseaddress into envelope headers before the CloudEvents mapper runs.
                opts.MetadataRules.Add(new CloudEventHeaderPolicy());

                // UserIdEnvelopeMiddleware stamps the HTTP user ID into envelope headers.
                // Resolves from DI so the shared IHttpContextAccessor instance is used.
                opts.Services.AddSingleton<IEnvelopeRule, UserIdEnvelopeMiddleware>(sp =>
                    sp.GetRequiredService<UserIdEnvelopeMiddleware>()
                );

                // ── Service-specific customisation ─────────────────────────────────
                configure?.Invoke(opts);

                // ── Kafka transport with CloudEvents interop ──────────────────────
                // Called after configure so per-service topic subscriptions can override
                // defaults before the Kafka transport is finalized.
                opts.UseKafkaWithCloudEvents(kafkaConnectionString, applicationName);
            });
        }
    }

    /// <summary>
    ///     Configures the Kafka transport on <paramref name="opts" /> with
    ///     CloudEvents interoperability applied to every endpoint and with
    ///     the per-service <c>source</c> URN derived from the application name.
    ///     Call this inside your <c>UseWolverineEventFramework</c> configure callback.
    /// </summary>
    /// <param name="opts">The <see cref="WolverineOptions" /> being configured.</param>
    /// <param name="kafkaConnectionString">Kafka bootstrap-server connection string.</param>
    /// <param name="applicationName">
    ///     Used to derive the CloudEvent <c>source</c> URN
    ///     (<c>urn:bookworm:{application-name}</c>). Typically
    ///     <c>IHostEnvironment.ApplicationName</c>.
    /// </param>
    public static void UseKafkaWithCloudEvents(
        this WolverineOptions opts,
        string kafkaConnectionString,
        string applicationName
    )
    {
        // The CloudEvent source URN per service, e.g. urn:bookworm:bookworm-catalog.
        // Wolverine uses ServiceName as the default `source` attribute when serialising
        // CloudEvents, so setting it to the URN form satisfies FR-003 / data-model.md § Entity 2.
        var kebabName = KafkaTopicRouter.ToKebabCase(applicationName);
        opts.ServiceName = $"urn:bookworm:{kebabName}";

        opts.UseKafka(kafkaConnectionString);
    }
}
