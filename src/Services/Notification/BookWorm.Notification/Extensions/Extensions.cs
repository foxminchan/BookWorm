namespace BookWorm.Notification.Extensions;

[ExcludeFromCodeCoverage]
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

        builder.Services.AddTransient<IRenderer, MjmlRenderer>();

        // Register the mailkit sender for development
        // and the sendgrid sender for other environments
        if (builder.Environment.IsDevelopment())
        {
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

            var emailOptions = new EmailOptions { From = string.Empty };

            services
                .AddOptionsWithValidateOnStart<EmailOptions>()
                .BindConfiguration(EmailOptions.ConfigurationSection)
                .ValidateDataAnnotations();

            services.AddSingleton(emailOptions);
        }
        else
        {
            var sendGirdOptions = new SendGirdOptions
            {
                ApiKey = string.Empty,
                SenderEmail = string.Empty,
                SenderName = string.Empty,
            };

            services
                .AddOptionsWithValidateOnStart<SendGirdOptions>()
                .BindConfiguration(SendGirdOptions.ConfigurationSection)
                .ValidateDataAnnotations();

            services.AddSingleton(sendGirdOptions);

            services.AddSingleton<ISender, SendGridSender>();

            services
                .AddHealthChecks()
                .AddCheck<SendGridHealthCheck>(nameof(SendGridHealthCheck), HealthStatus.Degraded);
        }

        builder.AddDefaultCors();

        services.AddOpenTelemetry().WithTracing(t => t.AddSource(TelemetryTags.ActivitySourceName));

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(INotificationApiMarker)]);
    }
}
