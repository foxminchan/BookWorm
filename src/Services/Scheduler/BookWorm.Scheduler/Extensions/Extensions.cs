using BookWorm.Constants.Aspire;
using Quartz.AspNetCore;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddEventBus(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(ISchedulerApiMarker).Assembly);
            });

            services.AddAntiforgery();

            services.AddQuartz(q =>
            {
                q.UseTimeZoneConverter();
                q.UseJobHistoryLogging();
                q.UseTriggerHistoryLogging();
                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = Environment.ProcessorCount);
                q.AddJobListener<JobTelemetryListener>();

                q.UseXmlSchedulingConfiguration(x =>
                {
                    x.Files.Add("~/jobs.xml");
                    x.ScanInterval = TimeSpan.FromMinutes(1);
                    x.FailOnFileNotFound = true;
                    x.FailOnSchedulingError = true;
                });
            });

            builder.AddQuartzPersistentStore(Components.Database.Scheduler);
            services.AddQuartzDashboard();

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}
