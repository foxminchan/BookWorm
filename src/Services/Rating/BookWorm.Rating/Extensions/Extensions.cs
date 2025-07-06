using BookWorm.Chassis.Mediator;
using BookWorm.Rating.Agents;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    /// <summary>
    /// Configures and registers all core application services, middleware, and infrastructure components required for the rating module.
    /// </summary>
    /// <remarks>
    /// This extension method sets up CORS, OpenAPI, authentication (with Keycloak claims transformation), rate limiting, exception handling, database context and migrations, MediatR with validation, FluentValidation, activity and metrics tracking, event bus with in-memory inbox/outbox, event mapping and dispatching, API versioning, endpoint registration, asynchronous API support, and agent services for the rating application.
    /// </remarks>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddRateLimiting();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddAzurePostgresDbContext<RatingDbContext>(
            Components.Database.Rating,
            _ =>
            {
                services.AddMigration<RatingDbContext>();

                services.AddRepositories(typeof(IRatingApiMarker));
            }
        );

        // Configure MediatR
        services.AddMediatR<IRatingApiMarker>(configuration =>
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>))
        );

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IRatingApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure EventBus first
        builder.AddEventBus(typeof(IRatingApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        // Then register event-related services
        services.AddScoped<IEventMapper, EventMapper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IRatingApiMarker));

        builder.AddDefaultAsyncApi([typeof(IRatingApiMarker)]);

        builder.AddAgents();
    }
}
