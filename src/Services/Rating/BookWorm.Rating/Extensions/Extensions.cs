namespace BookWorm.Rating.Extensions;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        services.AddDbContext<RatingDbContext>(options =>
        {
            options
                .UseNpgsql(builder.Configuration.GetConnectionString(Components.Database.Rating))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
        });
        builder.EnrichAzureNpgsqlDbContext<RatingDbContext>();

        services.AddMigration<RatingDbContext>();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IRatingApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IRatingApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        services.AddRepositories(typeof(IRatingApiMarker));

        // Configure EventBus first
        builder.AddEventBus(typeof(IRatingApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        // Then register event-related services
        services.AddScoped<IEventMapper, EventMapper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IRatingApiMarker));

        builder.AddDefaultAsyncApi([typeof(IRatingApiMarker)]);
    }
}
