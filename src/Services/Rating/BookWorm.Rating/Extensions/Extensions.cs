using BookWorm.Chassis.Mediator;
using BookWorm.Rating.Agents;
using BookWorm.Rating.Infrastructure.Summarizer;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
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
        services.AddSingleton<ISummarizer, RatingSummarizer>();
    }
}
