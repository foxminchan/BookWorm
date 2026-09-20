using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Scheduler.Jobs.Listeners;

[ExcludeFromCodeCoverage]
internal sealed class JobTelemetryListener(ILogger<JobTelemetryListener> logger) : IJobListener
{
    private static readonly ConcurrentDictionary<string, long> _jobStartTimestamps = new();

    public string Name => nameof(JobTelemetryListener);

    public ValueTask JobToBeExecuted(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        _jobStartTimestamps[context.FireInstanceId] = Stopwatch.GetTimestamp();

        logger.LogInformation(
            "Job {JobName} in group {JobGroup} starting. Fire instance: {FireInstanceId}, Scheduled fire time: {ScheduledFireTime:O}",
            context.JobDetail.Key.Name,
            context.JobDetail.Key.Group,
            context.FireInstanceId,
            context.ScheduledFireTimeUtc
        );

        return ValueTask.CompletedTask;
    }

    public ValueTask JobExecutionVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        _jobStartTimestamps.TryRemove(context.FireInstanceId, out _);

        logger.LogWarning(
            "Job {JobName} in group {JobGroup} was vetoed. Fire instance: {FireInstanceId}",
            context.JobDetail.Key.Name,
            context.JobDetail.Key.Group,
            context.FireInstanceId
        );

        return ValueTask.CompletedTask;
    }

    public ValueTask JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default
    )
    {
        var elapsed = _jobStartTimestamps.TryRemove(context.FireInstanceId, out var start)
            ? Stopwatch.GetElapsedTime(start)
            : TimeSpan.Zero;

        if (jobException is not null)
        {
            logger.LogError(
                jobException,
                "Job {JobName} in group {JobGroup} failed after {ElapsedMs:F1}ms. Fire instance: {FireInstanceId}",
                context.JobDetail.Key.Name,
                context.JobDetail.Key.Group,
                elapsed.TotalMilliseconds,
                context.FireInstanceId
            );
        }
        else
        {
            logger.LogInformation(
                "Job {JobName} in group {JobGroup} completed in {ElapsedMs:F1}ms. Fire instance: {FireInstanceId}, Next fire time: {NextFireTime:O}",
                context.JobDetail.Key.Name,
                context.JobDetail.Key.Group,
                elapsed.TotalMilliseconds,
                context.FireInstanceId,
                context.NextFireTimeUtc
            );
        }

        return ValueTask.CompletedTask;
    }
}
