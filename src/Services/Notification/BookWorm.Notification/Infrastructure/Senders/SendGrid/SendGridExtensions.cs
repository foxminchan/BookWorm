using BookWorm.Chassis.Utilities.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.SendGrid;

internal static class SendGridExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the SendGrid client and related services using the default configuration section.
        /// </summary>
        public void AddSendGridClient()
        {
            AddSendGridSender(builder, SendGridSettings.ConfigurationSection);
        }

        private void AddSendGridSender(string configurationSection)
        {
            var services = builder.Services;

            services.Configure<SendGridSettings>(configurationSection);

            services.AddSendGrid(
                (sp, options) => options.ApiKey = sp.GetRequiredService<SendGridSettings>().ApiKey
            );

            services.AddTransient<ISender, SendGridSender>();

            services
                .AddHealthChecks()
                .AddSendGrid(
                    sp => sp.GetRequiredService<SendGridSettings>().ApiKey,
                    nameof(SendGrid),
                    HealthStatus.Degraded
                );
        }
    }

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
}
