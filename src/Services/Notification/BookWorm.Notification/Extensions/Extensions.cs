using BookWorm.Chassis.EF;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Repository;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.Outbox;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;
using Wolverine.EntityFrameworkCore;
using Wolverine.Persistence;
using Wolverine.Postgresql;

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
            new JsonSerializerOptions { Converters = { DateOnlyJsonConverter.Instance } }
        );

        services.AddActivityScope();

        builder.AddAzurePostgresDbContext<NotificationDbContext>(
            Components.Database.Notification,
            _ =>
            {
                services.AddMigration<NotificationDbContext>();

                services.AddRepositories(typeof(INotificationApiMarker));
            },
            true
        );

        // Resilience pipeline for the notification service
        builder.AddMailResiliencePipeline();

        services.AddSingleton<IRenderer, MjmlTemplateRenderer>();

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

        builder.AddEventBus(
            typeof(INotificationApiMarker),
            options =>
            {
                var connectionString = builder.Configuration.GetRequiredConnectionString(
                    Components.Database.Notification
                );

                options.PersistMessagesWithPostgresql(connectionString);

                options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

                options.Policies.AutoApplyTransactions();
            }
        );
    }
}
