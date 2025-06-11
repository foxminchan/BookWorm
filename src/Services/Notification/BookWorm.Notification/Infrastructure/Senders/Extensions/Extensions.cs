using BookWorm.Constants.Aspire;
using BookWorm.Notification.Infrastructure.Senders.Factories;
using BookWorm.ServiceDefaults.Configuration;
using SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.Extensions;

public static class Extensions
{
    public static void AddMailKitSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<MailKitSender>();

        var uri = builder.GetMailPitEndpoint(Components.MailPit);

        services.AddSingleton<ISmtpClientFactory>(_ => new SmtpClientFactory(uri));

        services.AddSingleton<ObjectPool<SmtpClient>>(sp =>
        {
            var factory = sp.GetRequiredService<ISmtpClientFactory>();
            var policy = new SmtpClientFactoryObjectPoolPolicy(factory);
            return ObjectPool.Create(policy);
        });

        services.Configure<EmailOptions>(EmailOptions.ConfigurationSection);

        services
            .AddHealthChecks()
            .AddSmtpHealthCheck(
                setup =>
                {
                    setup.Host = uri.Host;
                    setup.Port = uri.Port;
                },
                nameof(Components.MailPit),
                HealthStatus.Unhealthy
            );
    }

    public static void AddSendGridSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<SendGridOptions>(SendGridOptions.ConfigurationSection);

        services.AddSingleton<SendGridSender>();

        var apiKey = services.BuildServiceProvider().GetRequiredService<SendGridOptions>().ApiKey;

        services.AddSingleton<ISendGridClient>(_ => new SendGridClient(apiKey));

        services.AddHealthChecks().AddSendGrid(apiKey, nameof(SendGrid), HealthStatus.Degraded);
    }

    public static void AddOutBoxSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddTableService();

        services.AddScoped<ISender>(sp =>
        {
            var tableService = sp.GetRequiredService<ITableService>();

            ISender underlyingSender = builder.Environment.IsDevelopment()
                ? sp.GetRequiredService<MailKitSender>()
                : sp.GetRequiredService<SendGridSender>();

            return new OutboxSender(tableService, underlyingSender);
        });
    }
}
