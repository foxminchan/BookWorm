using BookWorm.Chassis.Caching;
using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Chassis.Logging;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Notification.Infrastructure.Senders.MailKit;

namespace BookWorm.Notification.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            services.AddActivityScope();

            builder.AddRedaction();

            builder.AddCaching();
            services.AddIntegrationEventIdempotency();

            builder.AddMailResiliencePipeline();

            services.AddSingleton<IRenderer, MjmlTemplateRenderer>();

            builder.AddMailKitClient(
                builder.Environment.IsDevelopment() ? Components.MailPit : Components.Email
            );

            builder.AddEventBus<INotificationApiMarker>(opts =>
                opts.MessagePartitioning.ByPropertyNamed(nameof(OrderId))
            );
        }
    }
}
