using BookWorm.Chassis.EventBus.Wolverine;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddEventBus<ISchedulerApiMarker>(opts =>
                opts.UsePostgresOutbox(builder.Configuration, Components.Database.Scheduler)
            );

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

                if (
                    !string.IsNullOrWhiteSpace(
                        builder.Configuration["Quartz:DashboardAgent:Endpoint"]
                    )
                )
                {
                    q.UseDashboardAgent(agent =>
                        agent.IsJobTypeAllowed = name =>
                            name.StartsWith("BookWorm.Scheduler.Jobs.", StringComparison.Ordinal)
                    );
                }
            });

            services.AddQuartzExecutionHistory(options =>
            {
                options.Retention = TimeSpan.FromDays(30);
                options.MaxEntriesPerScheduler = 100_000;
            });

            builder.AddAzureNpgsqlDataSource(Components.Database.Scheduler);

            builder.AddQuartzPersistentStore(Components.Database.Scheduler);

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}
