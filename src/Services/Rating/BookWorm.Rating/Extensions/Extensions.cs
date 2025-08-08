using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Mediator;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Rating.Agents;
using BookWorm.Rating.Infrastructure.Summarizer;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        builder.AddDefaultOpenApi();

        services.AddRateLimiting();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
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
        builder.AddEventBus(
            typeof(IRatingApiMarker),
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<RatingDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });

                cfg.AddConfigureEndpointsCallback(
                    (context, _, configurator) =>
                        configurator.UseEntityFrameworkOutbox<RatingDbContext>(context)
                );
            }
        );

        // Then register event-related services
        services.AddScoped<IEventMapper, EventMapper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IRatingApiMarker));

        builder.AddAgents();
        services.AddSingleton<ISummarizer, RatingSummarizer>();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
