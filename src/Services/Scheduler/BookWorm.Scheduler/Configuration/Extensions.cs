using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Quartz.AspNetCore;

namespace BookWorm.Scheduler.Configuration;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddEventBus(typeof(ISchedulerApiMarker));

        services
            .AddOptions<QuartzOptions>()
            .BindConfiguration(QuartzOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<QuartzOptions>, QuartzOptionsValidator>();

        services.AddQuartz(q =>
        {
            q.SchedulerName = nameof(Scheduler);
            q.SchedulerId = $"{nameof(BookWorm)}-{nameof(Scheduler)}-{Environment.MachineName}";
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = Environment.ProcessorCount);
            q.AddJobListener<JobTelemetryListener>();
            q.AddJobsFromConfiguration(builder.Configuration);
        });

        services.AddQuartzServer(q => q.WaitForJobsToComplete = true);

        services
            .AddHealthChecks()
            .AddCheck<QuartzHealthCheck>("quartz-scheduler", HealthStatus.Unhealthy, ["ready"]);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }

    private static void AddJobsFromConfiguration(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration config
    )
    {
        var section = config.GetSection(QuartzOptions.SectionName);

        foreach (var entry in section.GetChildren())
        {
            var jobTypeName = entry.Key;
            var cronExpression = entry.Value;

            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                continue;
            }

            var jobType = typeof(ISchedulerApiMarker)
                .Assembly.GetTypes()
                .FirstOrDefault(t =>
                    t.Name == jobTypeName && !t.IsAbstract && t.IsAssignableTo(typeof(IJob))
                );

            if (jobType is null)
            {
                continue;
            }

            var key = new JobKey(jobTypeName, nameof(Scheduler));

            quartz.AddJob(jobType, key, opts => opts.WithIdentity(key));

            quartz.AddTrigger(opts =>
                opts.ForJob(key)
                    .WithIdentity(key.Name)
                    .WithCronSchedule(
                        cronExpression,
                        x =>
                            x.InTimeZone(TimeZoneInfo.Utc)
                                .WithMisfireHandlingInstructionFireAndProceed()
                    )
            );
        }
    }
}
