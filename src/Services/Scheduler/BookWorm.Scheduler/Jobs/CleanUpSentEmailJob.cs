using BookWorm.Contracts;

namespace BookWorm.Scheduler.Jobs;

public sealed class CleanUpSentEmailJob(IBus bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await bus.Publish(new CleanUpSentEmailIntegrationEvent(), context.CancellationToken);
    }
}
