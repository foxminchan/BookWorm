namespace BookWorm.Scheduler.Infrastructure;

public interface ISchedulerDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
