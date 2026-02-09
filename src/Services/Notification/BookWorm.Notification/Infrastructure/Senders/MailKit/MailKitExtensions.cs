using MailKit;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal static class MailKitExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers and configures the MailKit client for dependency injection using the specified connection name.
        /// </summary>
        /// <param name="connectionName">The name of the connection string to use for MailKit configuration.</param>
        /// <param name="configureSettings">
        ///     An optional delegate to further configure <see cref="MailKitSettings" /> after loading from configuration and
        ///     connection string.
        /// </param>
        public void AddMailKitClient(
            string connectionName,
            Action<MailKitSettings>? configureSettings = null
        )
        {
            builder.AddMailKitSender(
                MailKitSettings.ConfigurationSection,
                configureSettings,
                connectionName
            );
        }

        private void AddMailKitSender(
            string sectionName,
            Action<MailKitSettings>? configureSettings,
            string connectionName
        )
        {
            var services = builder.Services;

            // Bind settings from configuration section
            var settings = new MailKitSettings();
            builder.Configuration.GetSection(sectionName).Bind(settings);

            // Parse connection string if available
            if (builder.Configuration.GetConnectionString(connectionName) is { } connectionString)
            {
                settings.ParseConnectionString(connectionString);
            }

            configureSettings?.Invoke(settings);

            // Register settings as singleton for injection
            services.AddSingleton(settings);

            // Register factory as singleton to manage SMTP connection state
            services.AddSingleton(_ => new MailKitClientFactory(settings));

            // Register both concrete type and interface for outbox pattern support
            services.AddScoped<MailKitSender>();
            services.AddScoped<ISender>(sp => sp.GetRequiredService<MailKitSender>());

            if (!settings.DisableHealthChecks)
            {
                services
                    .AddHealthChecks()
                    .AddCheck<MailKitHealthCheck>(nameof(MailKit), null, [connectionName]);
            }

            if (!settings.DisableTracing)
            {
                services
                    .AddOpenTelemetry()
                    .WithTracing(traceBuilder =>
                        traceBuilder.AddSource(Telemetry.SmtpClient.ActivitySourceName)
                    );
            }

            if (!settings.DisableMetrics)
            {
                Telemetry.SmtpClient.Configure();

                services
                    .AddOpenTelemetry()
                    .WithMetrics(metricsBuilder =>
                        metricsBuilder.AddMeter(Telemetry.SmtpClient.MeterName)
                    );
            }
        }
    }
}
