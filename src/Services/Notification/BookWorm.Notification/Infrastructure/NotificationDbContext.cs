using BookWorm.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Infrastructure;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options),
        INotificationDbContext
{
    public DbSet<Outbox> Outboxes => Set<Outbox>();
}
