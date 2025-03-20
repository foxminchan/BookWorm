using System.Net.Mail;

namespace BookWorm.Notification.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Resilience pipeline for the notification service
        services.AddResiliencePipeline(
            nameof(Notification),
            pipelineBuilder =>
            {
                pipelineBuilder
                    .AddRetry(
                        new()
                        {
                            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                            BackoffType = DelayBackoffType.Exponential,
                            MaxRetryAttempts = 3,
                            Delay = TimeSpan.FromSeconds(2),
                            UseJitter = true,
                        }
                    )
                    .AddTimeout(TimeSpan.FromSeconds(45));
            }
        );

        // If the application is running in development mode, use the local SMTP server
        // Otherwise, use SendGrid for sending emails
        if (builder.Environment.IsDevelopment())
        {
            services.AddSingleton<ISmtpClient, SmtpClientWithOpenTelemetry>();

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

                var smtpClient = new SmtpClient(smtpUri.Host, smtpUri.Port);

                if (!string.Equals(smtpUri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                {
                    smtpClient.EnableSsl = true;
                }

                smtpClient.Credentials = new NetworkCredential(smtpUri.UserName, smtpUri.Password);
                return smtpClient;
            });

            var emailOptions = new EmailOptions { From = string.Empty };

            services
                .AddOptions<EmailOptions>()
                .BindConfiguration("Email")
                .ValidateDataAnnotations();

            services.AddSingleton(emailOptions);
        }
        else
        {
            services.AddSingleton<ISmtpClient, SendGridClient>();

            var sendGirdOptions = new SendGirdOptions
            {
                ApiKey = string.Empty,
                SenderEmail = string.Empty,
                SenderName = string.Empty,
            };

            services
                .AddOptions<SendGirdOptions>()
                .BindConfiguration("SendGrid")
                .ValidateDataAnnotations();

            services.AddSingleton(sendGirdOptions);

            services
                .AddHealthChecks()
                .AddCheck<SendGridHealthCheck>(nameof(SendGridHealthCheck), HealthStatus.Degraded);
        }

        services.AddOpenTelemetry().WithTracing(t => t.AddSource(TelemetryTags.ActivitySourceName));

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        services.AddAsyncApiDocs([typeof(INotificationApiMarker)], nameof(Notification));
    }
}
