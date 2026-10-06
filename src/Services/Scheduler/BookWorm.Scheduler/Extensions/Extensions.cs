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
