using BookWorm.ServiceDefaults.Configuration;
using OpenTelemetry.Trace;

namespace BookWorm.Notification.Infrastructure.Job;

public static class Extensions
{
    private static void AddJobConfigurator<TJob>(
        this IServiceCollectionQuartzConfigurator quartz,
        string cronExpression,
        TimeZoneInfo? timeZoneInfo = null,
        Action<ITriggerConfigurator>? trigger = null
    )
        where TJob : IJob
    {
        timeZoneInfo ??= TimeZoneInfo.Utc;

        var key = new JobKey(typeof(TJob).Name, nameof(Notification));

        quartz.AddJob<TJob>(opts => opts.WithIdentity(key));

        quartz.AddTrigger(opts =>
        {
            opts.ForJob(key)
                .WithIdentity(key.Name)
                .WithCronSchedule(cronExpression, x => x.InTimeZone(timeZoneInfo));

            trigger?.Invoke(opts);
        });
    }

    public static void AddCronJobServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<JobOptions>(JobOptions.ConfigurationSection);

        var jobOptions = services.BuildServiceProvider().GetRequiredService<JobOptions>();

        services.AddQuartz(q =>
        {
            q.AddJobConfigurator<CleanUpSentEmailWorker>(jobOptions.CleanUpSentEmailCronExpression);
            q.AddJobConfigurator<ResendErrorEmailWorker>(jobOptions.ResendErrorEmailCronExpression);
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }
}
