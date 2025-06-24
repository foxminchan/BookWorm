using BookWorm.Constants.Aspire;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.Outbox;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;

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

        services.AddSingleton<IActivityScope, ActivityScope>();

        // Resilience pipeline for the notification service
        builder.AddMailResiliencePipeline();

        builder.Services.AddTransient<IRenderer, MjmlRenderer>();

        // Register the mailkit sender for development
        // and the sendgrid sender for other environments
        if (builder.Environment.IsDevelopment())
        {
            builder.AddMailKitClient(Components.MailPit);
        }
        else
        {
            builder.AddSendGridClient();
        }

        builder.AddEmailOutbox();

        builder.AddCronJobServices();

        builder.AddEventBus(typeof(INotificationApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(INotificationApiMarker)]);
    }
}
