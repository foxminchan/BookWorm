using BookWorm.Chassis.EF;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.Outbox;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Extensions;

internal static class Extensions
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

        builder.AddAzureNpgsqlDbContext<NotificationDbContext>(
            Components.Database.Notification,
            configureDbContextOptions: options => options.UseSnakeCaseNamingConvention()
        );

        services.AddMigration<NotificationDbContext>();

        services.AddScoped<INotificationDbContext>(sp =>
            sp.GetRequiredService<NotificationDbContext>()
        );

        // Resilience pipeline for the notification service
        builder.AddMailResiliencePipeline();

        services.AddTransient<IRenderer, MjmlRenderer>();

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
    }
}
