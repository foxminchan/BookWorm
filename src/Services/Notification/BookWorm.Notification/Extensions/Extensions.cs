using BookWorm.Notification.Infrastructure.Senders.Providers;

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
            builder.AddMailKitSender();
        }
        else
        {
            builder.AddSendGridSender();
        }

        builder.AddAzureTableClient(Components.Azure.Storage.Table);
        services.AddScoped<ITableService, TableService>();
        services.Decorate<ISender, OutboxSender>();

        services.AddHostedService<ResendErrorEmailWorker>();
        services.AddHostedService<CleanUpSentEmailWorker>();

        builder.AddDefaultCors();

        services.AddOpenTelemetry().WithTracing(t => t.AddSource(TelemetryTags.ActivitySourceName));

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(INotificationApiMarker)]);
    }
}
