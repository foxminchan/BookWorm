using BookWorm.Chassis.EF;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Repository;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.Outbox;

namespace BookWorm.Notification.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            services.AddActivityScope();

            builder.AddAzurePostgresDbContext<NotificationDbContext>(
                Components.Database.Notification,
                _ =>
                {
                    services.AddMigration<NotificationDbContext>();

                    services.AddRepositories(typeof(NotificationApiMarker));
                },
                true
            );

            // Resilience pipeline for the notification service
            builder.AddMailResiliencePipeline();

            services.AddSingleton<IRenderer, MjmlTemplateRenderer>();

            builder.AddMailKitClient(
                builder.Environment.IsDevelopment() ? Components.MailPit : Components.Email
            );

            builder.AddEmailOutbox();

            builder.AddEventBus<NotificationApiMarker>(opts =>
            {
                opts.UsePostgresOutbox(builder.Configuration, Components.Database.Notification);

                // Preserve per-order saga ordering for outbound order emails by
                // routing all events that expose an OrderId to the same partition.
                opts.MessagePartitioning.ByPropertyNamed("OrderId");
            });
        }
    }
}
