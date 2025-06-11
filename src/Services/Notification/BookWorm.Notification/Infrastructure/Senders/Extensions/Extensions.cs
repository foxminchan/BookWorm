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

        services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();

        services.AddSingleton<ObjectPool<SmtpClient>>(sp =>
        {
            var factory = sp.GetRequiredService<ISmtpClientFactory>();
            var policy = new SmtpClientFactoryObjectPoolPolicy(factory);
            return ObjectPool.Create(policy);
        });

        services.Configure<EmailOptions>(EmailOptions.ConfigurationSection);
    }

    public static void AddSendGridSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<SendGridOptions>(SendGridOptions.ConfigurationSection);

        services.AddSingleton<SendGridSender>();

        services.AddSingleton<ISendGridClient>(sp => new SendGridClient(
            sp.GetRequiredService<SendGridOptions>().ApiKey
        ));

        services
            .AddHealthChecks()
            .AddCheck<SendGridHealthCheck>(nameof(SendGridHealthCheck), HealthStatus.Degraded);
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
