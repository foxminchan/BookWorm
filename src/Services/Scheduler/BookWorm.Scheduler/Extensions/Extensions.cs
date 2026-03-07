using OpenTelemetry.Trace;
using Quartz.AspNetCore;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddEventBus(typeof(ISchedulerApiMarker));

        services.AddAntiforgery();

        services.Configure<QuartzOptions>(options =>
        {
            options["quartz.plugin.jobHistory.type"] =
                "Quartz.Plugin.History.LoggingJobHistoryPlugin, Quartz.Plugins";
            options["quartz.plugin.triggerHistory.type"] =
                "Quartz.Plugin.History.LoggingTriggerHistoryPlugin, Quartz.Plugins";
        });

        services.AddQuartz(q =>
        {
            q.SchedulerId = $"{nameof(BookWorm)}-{nameof(Scheduler)}-{Environment.MachineName}";
            q.SchedulerName = nameof(Scheduler);

            q.InterruptJobsOnShutdown = true;
            q.InterruptJobsOnShutdownWithWait = true;

            q.UseTimeZoneConverter();
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = Environment.ProcessorCount);
            q.AddJobListener<JobTelemetryListener>();

            q.UseXmlSchedulingConfiguration(x =>
            {
                x.Files = ["~/jobs.xml"];
                x.ScanInterval = TimeSpan.FromMinutes(1);
                x.FailOnFileNotFound = true;
                x.FailOnSchedulingError = true;
            });
        });

        services.AddQuartzDashboard();

        services.AddQuartzServer(q => q.WaitForJobsToComplete = true);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }
}
