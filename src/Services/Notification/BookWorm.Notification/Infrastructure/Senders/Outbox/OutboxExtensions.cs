using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders.MailKit;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

internal static class OutboxExtensions
{
    /// <summary>
    ///     Registers the email outbox service and its dependencies in the application's service container.
    ///     Chooses the underlying email sender implementation based on the current environment:
    ///     <list type="bullet">
    ///         <item>Uses <see cref="MailKitSender" /> in development.</item>
    ///         <item>Uses <see cref="SendGridSender" /> otherwise.</item>
    ///     </list>
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to configure.</param>
    public static void AddEmailOutbox(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISender>(sp =>
        {
            var repository = sp.GetRequiredService<IOutboxRepository>();

            ISender underlyingSender = builder.Environment.IsDevelopment()
                ? sp.GetRequiredService<MailKitSender>()
                : sp.GetRequiredService<SendGridSender>();

            return new EmailOutboxService(repository, underlyingSender);
        });
    }
}
