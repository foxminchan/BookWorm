using TickerQ.Dashboard.DependencyInjection;
using TickerQ.Instrumentation.OpenTelemetry;

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

        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(nameof(TickerQ)));

        services.AddTickerQ(options =>
        {
            options.ConfigureScheduler(schedulerOptions =>
            {
                schedulerOptions.MaxConcurrency = Environment.ProcessorCount;
                schedulerOptions.NodeIdentifier = Environment.MachineName;
            });

            options.AddDashboard(dashboardOptions =>
            {
                dashboardOptions.SetBasePath("/admin/tickerq");

                if (builder.Environment.IsDevelopment())
                {
                    dashboardOptions.WithNoAuth();
                }
                else
                {
                    dashboardOptions.WithApiKey(builder.Configuration["TickerQ__ApiKey"]);
                }
            });

            options.AddOpenTelemetryInstrumentation();
        });
    }
}
