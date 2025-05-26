namespace BookWorm.Notification.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

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

        builder.AddOutBoxSender();

        builder.AddCronJobServices();

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(INotificationApiMarker)]);
    }
}
