namespace BookWorm.Notification.Infrastructure.Job;

public static class Extensions
{
    /// <summary>
    ///     Configures a scheduled job in Quartz with the specified cron expression.
    /// </summary>
    /// <typeparam name="TJob">The type of job to be scheduled. Must implement IJob interface.</typeparam>
    /// <param name="quartz">The Quartz configurator extension point.</param>
    /// <param name="cronExpression">A cron expression that defines the job's execution schedule.</param>
    /// <param name="timeZoneInfo">
    ///     The timezone in which the cron expression should be evaluated. Defaults to UTC if not
    ///     specified.
    /// </param>
    /// <param name="trigger">Optional action to further configure the trigger.</param>
    /// <remarks>
    ///     This method simplifies Quartz job registration by automatically creating a job key based on the job type name
    ///     and setting up the appropriate triggers with the provided cron schedule.
    /// </remarks>
    public static void AddJobConfigurator<TJob>(
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
}
