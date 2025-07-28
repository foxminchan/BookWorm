using BookWorm.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Infrastructure;

public interface INotificationDbContext
{
    DbSet<Outbox> Outboxes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
