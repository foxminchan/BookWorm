using BookWorm.Chassis.Repository;
using BookWorm.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Infrastructure;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Outbox> Outboxes => Set<Outbox>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
