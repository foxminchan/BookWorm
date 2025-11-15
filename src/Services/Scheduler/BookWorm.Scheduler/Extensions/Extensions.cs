using OpenTelemetry.Trace;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddEventBus(typeof(ISchedulerApiMarker));

        services.AddQuartz(q =>
        {
            q.SchedulerName = nameof(Scheduler);
            q.SchedulerId = $"{nameof(BookWorm)}-{nameof(Scheduler)}-{Environment.MachineName}";
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = Environment.ProcessorCount);
            q.AddJobConfigurator<CleanUpSentEmailJob>(
                builder.Configuration[$"{nameof(Quartz)}:{nameof(CleanUpSentEmailJob)}"]
            );
            q.AddJobConfigurator<ResendErrorEmailJob>(
                builder.Configuration[$"{nameof(Quartz)}:{nameof(ResendErrorEmailJob)}"]
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }

    private static void AddJobConfigurator<TJob>(
        this IServiceCollectionQuartzConfigurator quartz,
        string? cronExpression,
        Action<ITriggerConfigurator>? trigger = null
    )
        where TJob : IJob
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cronExpression);

        var key = new JobKey(typeof(TJob).Name, nameof(Scheduler));

        quartz.AddJob<TJob>(opts => opts.WithIdentity(key));

        quartz.AddTrigger(opts =>
        {
            opts.ForJob(key)
                .WithIdentity(key.Name)
                .WithCronSchedule(cronExpression, x => x.InTimeZone(TimeZoneInfo.Utc));

            trigger?.Invoke(opts);
        });
    }
}
