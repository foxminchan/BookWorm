using System.Text.Json;
using BookWorm.Constants.Aspire;
using JasperFx;
using JasperFx.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Postgresql;

namespace BookWorm.Chassis.EventBus.Wolverine;

/// <summary>
///     Extension methods that wire WolverineFx into a BookWorm microservice host.
///     Replaces the MassTransit-based <c>AddEventBus</c> bootstrap (H-3 in
///     <c>contracts/handler-conventions.md</c>).
/// </summary>
public static class WolverineHostExtensions
{
    private static readonly JsonSerializerOptions _cloudEventJsonOptions = new(
        JsonSerializerDefaults.Web
    );

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
        public void UseBookWormWolverine(Action<WolverineOptions>? configure = null)
        {
            var kafkaConnectionString = builder.Configuration.GetConnectionString(
                Components.Broker
            );

            // Skip Wolverine/Kafka registration when no broker is configured (FR-015).
            if (string.IsNullOrWhiteSpace(kafkaConnectionString))
            {
                return;
            }

            // IHttpContextAccessor is a singleton that uses AsyncLocal internally.
            // Adding it here ensures downstream code can inject it without a separate call.
            builder.Services.AddHttpContextAccessor();

            // UserIdEnvelopeMiddleware needs the IHttpContextAccessor singleton.
            // HttpContextAccessor shares the same static AsyncLocal as the framework-
            // registered instance so creating a second instance is safe.
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
                // defaults before the Kafka transport is finalised.
                opts.UseKafkaWithCloudEvents(kafkaConnectionString!, applicationName);
            });
        }
    }

    /// <summary>
    ///     Configures the Kafka transport on <paramref name="opts" /> with
    ///     CloudEvents interoperability applied to every endpoint and with
    ///     the per-service <c>source</c> URN derived from the application name.
    ///     Call this inside your <c>UseBookWormWolverine</c> configure callback.
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
