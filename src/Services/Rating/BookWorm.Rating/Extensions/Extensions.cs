using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Core;
using BookWorm.Rating.Configurations;
using BookWorm.Rating.Infrastructure.Agents;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Wolverine.EntityFrameworkCore;
using Wolverine.Persistence;
using Wolverine.Postgresql;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddAppSettings<RatingAppSettings>();

        builder.AddDefaultAuthentication().WithKeycloakClaimsTransformation();

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                Authorization.Policies.Admin,
                policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Rating}_{Authorization.Actions.Read}",
                            $"{Services.Rating}_{Authorization.Actions.Write}"
                        );
                }
            );

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure Mediator
        services
            .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
            .ApplyLoggingBehavior()
            .ApplyActivityBehavior()
            .ApplyValidationBehavior();

        builder.AddRateLimiting();

        // Add database configuration
        builder.AddAzurePostgresDbContext<RatingDbContext>(
            Components.Database.Rating,
            _ =>
            {
                services.AddMigration<RatingDbContext>();

                services.AddRepositories(typeof(IRatingApiMarker));
            }
        );

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IRatingApiMarker>(includeInternalTypes: true);

        services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

        // Configure EventBus first
        builder.AddEventBus(
            typeof(IRatingApiMarker),
            options =>
            {
                var connectionString = builder.Configuration.GetRequiredConnectionString(
                    Components.Database.Rating
                );

                options.PersistMessagesWithPostgresql(connectionString);

                options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

                options.Policies.AutoApplyTransactions();
            }
        );

        // Then register event-related services
        services.AddEventDispatcher();
        services.AddScoped<IEventMapper, EventMapper>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IRatingApiMarker));
        services.AddDefaultOpenApi(options =>
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<RatingAppSettings>>()
        );

        builder.AddAgents();
        services.AddScoped<ISummarizer, RatingSummarizer>();

        services.AddKeycloakTokenIntrospection();
    }
}
