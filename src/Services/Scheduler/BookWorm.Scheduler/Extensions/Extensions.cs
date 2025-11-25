using OpenTelemetry.Trace;
using Quartz.AspNetCore;

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
            q.AddJobAndTrigger<CleanUpSentEmailJob>(builder.Configuration);
            q.AddJobAndTrigger<ResendErrorEmailJob>(builder.Configuration);
        });

        services.AddQuartzServer(q => q.WaitForJobsToComplete = true);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }

    private static void AddJobAndTrigger<TJob>(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration config,
        Action<ITriggerConfigurator>? trigger = null
    )
        where TJob : IJob
    {
        var key = new JobKey(typeof(TJob).Name, nameof(Scheduler));

        var configKey = $"{nameof(Quartz)}:{key.Name}";
        var cronExpression = config[configKey];

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException(
                $"Cron schedule not found in configuration for key: {configKey}"
            );
        }

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
