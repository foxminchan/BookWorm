using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using BookWorm.Scheduler.Clients;
using BookWorm.ServiceDefaults.Kestrel;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            services
                .AddHttpServiceReference<ICatalogIngestionApi>(
                    HttpUtilities
                        .AsUrlBuilder()
                        .WithScheme(Http.Schemes.HttpOrHttps)
                        .WithHost(Services.CatalogIngestion)
                        .Build(),
                    CatalogIngestionSerializationContext.Default
                )
                .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(5))
                .RemoveAllResilienceHandlers();

            builder.AddEventBus(opts =>
                opts.Discovery.IncludeAssembly(typeof(ISchedulerApiMarker).Assembly)
            );

            services.AddAntiforgery();

            services.AddQuartz(
                builder.Configuration.GetSection("Quartz"),
                q =>
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
                }
            );

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
