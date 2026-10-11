using BookWorm.Chassis.Utilities.Configurations;
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

            var rawSettings = new MailKitSettings();
            builder.Configuration.GetSection(sectionName).Bind(rawSettings);

            builder.Configure<MailKitSettings>(sectionName);

            services.PostConfigure<MailKitSettings>(options =>
            {
                if (
                    builder.Configuration.GetConnectionString(connectionName) is
                    { } connectionString
                )
                {
                    options.ParseConnectionString(connectionString);
                }

                configureSettings?.Invoke(options);
            });

            services.AddSingleton<
                IValidateOptions<MailKitSettings>,
                MailKitSettings.MailKitCredentialsValidator
            >();

            // Register factory as singleton to manage SMTP connection state
            services.AddSingleton(sp => new MailKitClientFactory(
                sp.GetRequiredService<MailKitSettings>()
            ));

            // Register the provider-neutral sender contract for the outbox.
            services.AddScoped<MailKitSender>();
            services.AddScoped<IEmailSender>(sp => sp.GetRequiredService<MailKitSender>());

            if (!rawSettings.DisableHealthChecks)
            {
                services
                    .AddHealthChecks()
                    .AddCheck<MailKitHealthCheck>(nameof(MailKit), null, [connectionName]);
            }

            if (!rawSettings.DisableTracing)
            {
                services
                    .AddOpenTelemetry()
                    .WithTracing(traceBuilder =>
                        traceBuilder.AddSource(Telemetry.SmtpClient.ActivitySourceName)
                    );
            }

            if (!rawSettings.DisableMetrics)
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
