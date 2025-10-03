using TickerQ.Dashboard.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;

namespace BookWorm.Scheduler.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add database configuration
        builder.AddAzurePostgresDbContext<SchedulerDbContext>(
            Components.Database.Scheduler,
            excludeDefaultInterceptors: true
        );

        services.AddScoped<ISchedulerDbContext>(sp => sp.GetRequiredService<SchedulerDbContext>());

        // Configure EventBus first
        builder.AddEventBus(
            typeof(ISchedulerApiMarker),
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<SchedulerDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });

                cfg.AddConfigureEndpointsCallback(
                    (context, _, configurator) =>
                        configurator.UseEntityFrameworkOutbox<SchedulerDbContext>(context)
                );
            }
        );

        services.AddTickerQ(opt =>
        {
            opt.SetMaxConcurrency(Environment.ProcessorCount);
            opt.SetInstanceIdentifier(Environment.MachineName);
            opt.UpdateMissedJobCheckDelay(TimeSpan.FromMinutes(5));
            opt.AddOperationalStore<SchedulerDbContext>(efOpt =>
            {
                efOpt.CancelMissedTickersOnAppStart();
                efOpt.UseModelCustomizerForMigrations();
            });
            opt.AddDashboard(config =>
            {
                config.BasePath = "/tickerq";
                config.EnableBasicAuth = true;
            });
        });
    }
}
