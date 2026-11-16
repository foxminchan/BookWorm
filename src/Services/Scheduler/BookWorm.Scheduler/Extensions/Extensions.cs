using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Chassis.Security.Extensions;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultAuthentication();

            builder.AddEventBus(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(ISchedulerApiMarker).Assembly);
                opts.ConfigureIntegrationEventTopicsIn(
                    builder.Environment,
                    typeof(ISchedulerApiMarker).Assembly
                );
            });

            services.AddAntiforgery();

            services.AddQuartz(builder.Configuration.GetSection("Quartz"), q =>
            {
                q.UseJobHistoryLogging();
                q.UseStructuredTriggerLogging();
                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = Environment.ProcessorCount);
                q.AddJobListener<JobTelemetryListener>();

                q.UseJsonSchedulingConfiguration(x =>
                {
                    x.Files.Add("~/jobs.json");
                    x.ScanInterval = TimeSpan.FromMinutes(1);
                    x.FailOnFileNotFound = true;
                    x.FailOnSchedulingError = true;
                });
            });

            services.AddQuartzExecutionHistory(options =>
            {
                options.Retention = TimeSpan.FromDays(30);
                options.MaxEntriesPerScheduler = 100_000;
            });

            builder.AddQuartzPersistentStore(Components.Database.Scheduler);
            services.AddQuartzDashboard();

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}
