using BookWorm.Contracts;
using TickerQ.Utilities.Base;

namespace BookWorm.Scheduler.Jobs;

public sealed class CleanUpSentEmailJob(IBus bus, ISchedulerDbContext dbContext)
{
    [TickerFunction($"{nameof(CleanUpSentEmailJob)}", "0 0 * * *")]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await bus.Publish(new CleanUpSentEmailIntegrationEvent(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
