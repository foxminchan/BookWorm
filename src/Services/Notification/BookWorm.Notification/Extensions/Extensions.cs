using OpenTelemetry.Trace;

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

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { new DateOnlyJsonConverter() } }
        );

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

        // Replace the Decorate call with a factory registration
        services.AddSingleton<ISender>(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
            var sender = sp.GetRequiredService<ISender>();
            return new OutboxSender(tableService, sender);
        });

        services.AddQuartz(q =>
        {
            // Clean up sent emails job - runs daily at midnight
            q.AddJobConfigurator<CleanUpSentEmailWorker>("0 0 0 * * ?");

            // Resend error emails job - runs every hour
            q.AddJobConfigurator<ResendErrorEmailWorker>("0 0 * * * ?");
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        builder.AddDefaultCors();

        services
            .AddOpenTelemetry()
            .WithTracing(t =>
                t.AddSource(TelemetryTags.ActivitySourceName).AddQuartzInstrumentation()
            );

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(INotificationApiMarker)]);
    }
}
