using BookWorm.Constants.Aspire;

namespace BookWorm.Finance.Infrastructure;

public static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAzureNpgsqlDbContext<FinanceDbContext>(
            Components.Database.Finance,
            configureDbContextOptions: options => options.UseSnakeCaseNamingConvention()
        );

        services.AddMigration<FinanceDbContext>();
    }

    public static void AddSagaStateMachineServices(this IHostApplicationBuilder builder)
    {
        builder.AddEventBus(
            typeof(IFinanceApiMarker),
            configurator =>
            {
                configurator
                    .AddSagaStateMachine<
                        OrderStateMachine,
                        OrderState,
                        OrderStateMachineDefinition
                    >()
                    .AddRepository(builder);

                configurator.AddEntityFrameworkOutbox<FinanceDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });
            }
        );
    }

    private static void AddRepository(
        this ISagaRegistrationConfigurator<OrderState> configurator,
        IHostApplicationBuilder builder
    )
    {
        configurator.EntityFrameworkRepository(config =>
        {
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
        });
    }
}
