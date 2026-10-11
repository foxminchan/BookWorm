using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

internal static class OutboxExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddEmailOutbox()
        {
            builder.Services.AddScoped<ISender>(sp =>
            {
                var repository = sp.GetRequiredService<IOutboxRepository>();
                var emailSender = sp.GetRequiredService<IEmailSender>();
                return new EmailOutboxService(repository, emailSender);
            });
        }
    }
}
