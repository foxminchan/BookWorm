using Microsoft.EntityFrameworkCore;

namespace BookWorm.Scheduler.Infrastructure;

public sealed class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options)
    : DbContext(options),
        ISchedulerDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
