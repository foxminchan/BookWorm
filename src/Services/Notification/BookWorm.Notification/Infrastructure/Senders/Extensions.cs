namespace BookWorm.Notification.Infrastructure.Senders;

public static class Extensions
{
    public static void AddMailKitSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<ISender, MailKitSender>();

        services.AddSingleton(sp =>
            ObjectPool.Create(new DependencyInjectionObjectPoolPolicy<SmtpClient>(sp))
        );

        UriBuilder? smtpUri = null;

        services.AddTransient(_ =>
        {
            smtpUri ??= new(
                builder.Configuration.GetConnectionString(Components.MailPit)
                    ?? throw new InvalidOperationException("SMTP URI is not configured.")
            );

            var smtpClient = new SmtpClient();

            smtpClient.Connect(smtpUri.Host, smtpUri.Port);

            if (
                !string.Equals(
                    smtpUri.Host,
                    Restful.Host.Localhost,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                smtpClient.SslProtocols = SslProtocols.Tls13;
            }

            smtpClient.Authenticate(smtpUri.UserName, smtpUri.Password);

            return smtpClient;
        });

        services.Configure<EmailOptions>(EmailOptions.ConfigurationSection);
    }

    public static void AddSendGridSender(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<SendGridOptions>(SendGridOptions.ConfigurationSection);

        services.AddSingleton<ISender, SendGridSender>();

        services
            .AddHealthChecks()
            .AddCheck<SendGridHealthCheck>(nameof(SendGridHealthCheck), HealthStatus.Degraded);
    }
}
