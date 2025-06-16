using BookWorm.ServiceDefaults.Configuration;
using SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.SendGrid;

public static class SendGridExtensions
{
    /// <summary>
    ///     Registers the SendGrid client and related services using the default configuration section.
    /// </summary>
    /// <param name="builder">The application builder to configure services for.</param>
    public static void AddSendGridClient(this IHostApplicationBuilder builder)
    {
        AddSendGridSender(builder, SendGridSettings.ConfigurationSection);
    }

    private static void AddSendGridSender(
        this IHostApplicationBuilder builder,
        string configurationSection
    )
    {
        var services = builder.Services;

        services.Configure<SendGridSettings>(configurationSection);

        services.AddSingleton<SendGridSender>();

        services.AddSingleton<ISendGridClient>(_ =>
        {
            var apiKey = services
                .BuildServiceProvider()
                .GetRequiredService<SendGridSettings>()
                .ApiKey;
            return new SendGridClient(apiKey);
        });

        services
            .AddHealthChecks()
            .AddSendGrid(
                sp => sp.GetRequiredService<SendGridSettings>().ApiKey,
                nameof(SendGrid),
                HealthStatus.Degraded
            );
    }
}
