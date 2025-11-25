using BookWorm.Chassis.Utilities.Configuration;
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
            AddMailKitSender(
                builder,
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

            services.Configure<MailKitSettings>(sectionName);

            var settings = services.BuildServiceProvider().GetRequiredService<MailKitSettings>();

            if (builder.Configuration.GetConnectionString(connectionName) is { } connectionString)
            {
                settings.ParseConnectionString(connectionString);
            }

            configureSettings?.Invoke(settings);

            services.AddScoped(_ => new MailKitClientFactory(settings));

            services.AddScoped<ISender, MailKitSender>();

            if (!settings.DisableHealthChecks)
            {
                services
                    .AddHealthChecks()
                    .AddCheck<MailKitHealthCheck>("MailKit", null, [Components.MailPit]);
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
