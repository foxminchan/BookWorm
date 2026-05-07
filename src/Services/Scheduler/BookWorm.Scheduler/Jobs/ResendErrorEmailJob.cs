using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class ResendErrorEmailJob(IMessageBus bus, ILogger<ResendErrorEmailJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await bus.PublishAsync(new ResendErrorEmailIntegrationEvent());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventName}",
                nameof(ResendErrorEmailIntegrationEvent)
            );
            throw new JobExecutionException(ex, false);
        }
    }
}
