using BookWorm.Chassis.Utilities.Configuration;
using MailKit;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

internal static class MailKitExtensions
{
    /// <summary>
    ///     Registers and configures the MailKit client for dependency injection using the specified connection name.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to add the MailKit client to.</param>
    /// <param name="connectionName">The name of the connection string to use for MailKit configuration.</param>
    /// <param name="configureSettings">
    ///     An optional delegate to further configure <see cref="MailKitSettings" /> after loading from configuration and
    ///     connection string.
    /// </param>
    public static void AddMailKitClient(
        this IHostApplicationBuilder builder,
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

    private static void AddMailKitSender(
        this IHostApplicationBuilder builder,
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
