using BookWorm.Chassis.EF;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Repository;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.Outbox;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

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

            var postgresCs = builder.Configuration.GetConnectionString(
                Components.Database.Notification
            );
            builder.AddEventBus(opts =>
            {
                if (!string.IsNullOrWhiteSpace(postgresCs))
                {
                    opts.PersistMessagesWithPostgresql(postgresCs, "wolverine");
                    opts.UseEntityFrameworkCoreTransactions();
                }

                opts.Discovery.IncludeAssembly(typeof(INotificationApiMarker).Assembly);
            });
        }
    }
}
