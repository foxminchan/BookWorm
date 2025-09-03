using BookWorm.Contracts;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Enums;

namespace BookWorm.Scheduler.Jobs;

public sealed class ResendErrorEmailJob(IBus bus, ISchedulerDbContext dbContext)
{
    [TickerFunction(
        $"{nameof(ResendErrorEmailJob)}",
        "%CronTicker:ResendErrorEmailJob%",
        TickerTaskPriority.High
    )]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await bus.Publish(new ResendErrorEmailIntegrationEvent(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
