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
using BookWorm.ServiceDefaults.Cors;
using Mediator;
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
            services.AddValidationExceptionHandler();
            services.AddNotFoundExceptionHandler();
            services.AddGlobalExceptionHandler();
            services.AddProblemDetails();

            // Configure Mediator
            services
                .AddMediator(
                    (MediatorOptions options) => options.ServiceLifetime = ServiceLifetime.Scoped
                )
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

                    services.AddRepositories(typeof(IRatingApiMarker));
                }
            );

            // Configure FluentValidation
            services.AddValidatorsFromAssemblyContaining<IRatingApiMarker>(
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

                opts.Discovery.IncludeAssembly(typeof(IRatingApiMarker).Assembly);
            });

            // Then register event-related services
            services.AddEventDispatcher();
            services.AddScoped<IEventMapper, EventMapper>();

            // Configure endpoints
            services.AddVersioning();
            services.AddEndpoints(typeof(IRatingApiMarker));
            services.AddDefaultOpenApi(options =>
                options.ApplyOpenApiInfoDefinitions<RatingAppSettings>()
            );

            builder.AddAgents();
            services.AddScoped<ISummarizer, RatingSummarizer>();

            services.AddKeycloakTokenIntrospection();
        }
    }
}
