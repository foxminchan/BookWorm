using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.SendGrid;

internal static class SendGridExtensions
{
    private static void AddSendGrid(
        this IServiceCollection services,
        Action<IServiceProvider, SendGridClientOptions> configureOptions
    )
    {
        services
            .AddOptions<SendGridClientOptions>()
            .Configure<IServiceProvider>((options, resolver) => configureOptions(resolver, options))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ApiKey),
                "SendGrid API key must be provided in the configuration."
            );

        services.TryAddTransient<ISendGridClient>(resolver =>
            resolver.GetRequiredService<InjectableSendGridClient>()
        );

        services.AddHttpClient<InjectableSendGridClient>();
    }

    extension(IHostApplicationBuilder builder)
    {
        public void AddSendGridClient()
        {
            var services = builder.Services;

            builder.Configure<SendGridSettings>(SendGridSettings.ConfigurationSection);

            services.AddSendGrid(
                (sp, options) => options.ApiKey = sp.GetRequiredService<SendGridSettings>().ApiKey
            );

            // Register both concrete type and interface for outbox pattern support
            services.AddTransient<SendGridSender>();
            services.AddTransient<ISender>(sp => sp.GetRequiredService<SendGridSender>());

            services
                .AddHealthChecks()
                .AddSendGrid(
                    sp => sp.GetRequiredService<SendGridSettings>().ApiKey,
                    nameof(SendGrid),
                    HealthStatus.Degraded
                );
        }
    }
}
