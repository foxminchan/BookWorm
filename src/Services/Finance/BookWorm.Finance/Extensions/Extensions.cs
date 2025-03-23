namespace BookWorm.Finance.Extensions;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        services.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IFinanceApiMarker>();
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        builder.AddNpgsqlDbContext<FinanceDbContext>(
            Components.Database.Finance,
            configureDbContextOptions: options => options.UseSnakeCaseNamingConvention()
        );

        services.AddMigration<FinanceDbContext>();

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
                    .EntityFrameworkRepository(config =>
                    {
                        config.AddDbContext<DbContext, FinanceDbContext>(
                            (_, optionsBuilder) =>
                            {
                                optionsBuilder
                                    .UseNpgsql(
                                        builder.Configuration.GetConnectionString(
                                            Components.Database.Finance
                                        )
                                    )
                                    .UseSnakeCaseNamingConvention();
                            }
                        );
                    });

                configurator.AddEntityFrameworkOutbox<FinanceDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });
            }
        );

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IFinanceApiMarker));

        services.AddAsyncApiDocs([typeof(IFinanceApiMarker)], nameof(Finance));
    }
}
