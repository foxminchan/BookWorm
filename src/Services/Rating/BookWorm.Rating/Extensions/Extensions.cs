using BookWorm.Chassis.Caching;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Rating.Configurations;
using BookWorm.Rating.Infrastructure.AI;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddAppSettings<RatingAppSettings>();

            builder.AddDefaultAuthentication(Services.Rating).WithKeycloakClaimsTransformation();

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
                )
                .AddPolicy(
                    Authorization.Policies.Read,
                    policy =>
                    {
                        policy
                            .RequireAuthenticatedUser()
                            .RequireScope($"{Services.Rating}_{Authorization.Actions.Read}");
                    }
                );

            // Add exception handlers
            services.AddValidationExceptionHandler();
            services.AddNotFoundExceptionHandler();
            services.AddGlobalExceptionHandler();
            services.AddProblemDetails();

            // Configure Mediator
            services
                .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
                .ApplyLoggingBehavior()
                .ApplyActivityBehavior()
                .ApplyValidationBehavior()
                .ApplyTransactionBehavior<RatingDbContext>();

            builder.AddRateLimiting();

            // Add database configuration
            builder.AddAzurePostgresDbContext<RatingDbContext>(
                Components.Database.Rating,
                _ =>
                {
                    services.AddMigration<RatingDbContext>();

                    services.AddRepositories(typeof(RatingApiMarker));
                }
            );

            // Configure Redis Cache
            builder
                .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
                .WithAzureAuthentication();

            builder.AddCaching();

            // Configure FluentValidation
            services.AddValidatorsFromAssemblyContaining<RatingApiMarker>(
                includeInternalTypes: true
            );

            services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

            // Configure EventBus first
            var postgresCs = builder.Configuration.GetConnectionString(Components.Database.Rating);
            builder.AddEventBus(opts =>
            {
                if (!string.IsNullOrWhiteSpace(postgresCs))
                {
                    opts.PersistMessagesWithPostgresql(postgresCs, "wolverine");
                    opts.UseEntityFrameworkCoreTransactions();
                }

                // Preserve per-book / per-feedback ordering on Kafka so concurrent
                // rating updates for the same book land on a single partition.
                opts.MessagePartitioning.ByPropertyNamed("BookId", "FeedbackId");

                opts.Discovery.IncludeAssembly(typeof(RatingApiMarker).Assembly);
                opts.ConfigureIntegrationEventTopicsIn(
                    builder.Environment,
                    typeof(RatingApiMarker).Assembly
                );
            });

            // Then register event-related services
            services.AddEventDispatcher();
            services.AddScoped<IEventMapper, EventMapper>();

            // Configure endpoints
            services.AddVersioning();
            services.AddEndpoints(typeof(RatingApiMarker));
            services.AddDefaultOpenApi(options =>
                options
                    .MapVogenTypesInRatingApiMarker()
                    .ApplyOpenApiInfoDefinitions<RatingAppSettings>()
            );

            builder.AddAgents();
            builder.AddSummarizer();

            services.AddKeycloakTokenIntrospection();
        }
    }
}
