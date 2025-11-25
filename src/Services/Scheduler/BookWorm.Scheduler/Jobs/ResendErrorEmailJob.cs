using BookWorm.Contracts;

namespace BookWorm.Scheduler.Jobs;

public sealed class ResendErrorEmailJob(IBus bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await bus.Publish(new ResendErrorEmailIntegrationEvent(), context.CancellationToken);
    }
}
