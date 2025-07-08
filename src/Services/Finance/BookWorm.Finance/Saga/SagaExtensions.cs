using BookWorm.Constants.Aspire;
using OpenTelemetry.Trace;
using Quartz;

namespace BookWorm.Finance.Saga;

[ExcludeFromCodeCoverage]
public static class SagaExtensions
{
    public static void AddSagaStateMachineServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<OrderStateMachineSettings>(
            OrderStateMachineSettings.ConfigurationSection
        );

        services.AddQuartz(q =>
        {
            q.SchedulerName = nameof(Finance);
            q.SchedulerId = $"{nameof(Finance)}Scheduler";
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
        });

        builder.AddEventBus(
            typeof(IFinanceApiMarker),
            configurator =>
            {
                configurator.AddPublishMessageScheduler();

                configurator.AddQuartzConsumers();

                configurator
                    .AddSagaStateMachine<
                        OrderStateMachine,
                        OrderState,
                        OrderStateMachineDefinition
                    >()
                    .AddSagaRepository(builder);

                configurator.SetEntityFrameworkSagaRepositoryProvider(o =>
                {
                    o.ExistingDbContext<FinanceDbContext>();
                    o.UsePostgres();
                });

                configurator.AddEntityFrameworkOutbox();
            },
            (_, configure) => configure.UsePublishMessageScheduler()
        );

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddOpenTelemetry().WithTracing(t => t.AddQuartzInstrumentation());
    }

    private static void AddEntityFrameworkOutbox(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddEntityFrameworkOutbox<FinanceDbContext>(o =>
        {
            o.QueryDelay = TimeSpan.FromSeconds(1);

            o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

            o.UsePostgres();

            o.UseBusOutbox();
        });

        configurator.AddConfigureEndpointsCallback(
            (context, _, cfg) => cfg.UseEntityFrameworkOutbox<FinanceDbContext>(context)
        );
    }

    private static void AddSagaRepository(
        this ISagaRegistrationConfigurator<OrderState> configurator,
        IHostApplicationBuilder builder
    )
    {
        configurator.EntityFrameworkRepository(config =>
        {
            config.ConcurrencyMode = ConcurrencyMode.Optimistic;

            config.AddDbContext<DbContext, FinanceDbContext>(
                (_, optionsBuilder) =>
                {
                    optionsBuilder
                        .UseNpgsql(
                            builder.Configuration.GetRequiredConnectionString(
                                Components.Database.Finance
                            )
                        )
                        .UseSnakeCaseNamingConvention();
                }
            );

            config.UsePostgres();
        });
    }
}
