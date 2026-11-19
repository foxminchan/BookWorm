using BookWorm.Chassis.Caching;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Rating.Configurations;
using BookWorm.Rating.Infrastructure.AI;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Identity.Web;
using Wolverine.CosmosDb;

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

            builder.AddDefaultAuthentication();

            services
                .AddAuthorizationBuilder()
                .AddPolicy(Authorization.Policies.Admin, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Rating}_{Authorization.Actions.Read}",
                            $"{Services.Rating}_{Authorization.Actions.Write}"
                        );
                })
                .AddPolicy(Authorization.Policies.Read, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireScope($"{Services.Rating}_{Authorization.Actions.Read}");
                });

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
                .ApplyValidationBehavior();

            builder.AddRateLimiting();

            // Add database configuration
            var cosmosCs = builder.Configuration.GetConnectionString(Components.Container.Feedbacks)
                ?? builder.Configuration.GetConnectionString(Components.Database.Rating);
            services.AddEventDispatchInterceptor();
            services.AddDbContext<RatingDbContext>(
                (sp, options) =>
                {
                    if (!string.IsNullOrWhiteSpace(cosmosCs))
                    {
                        options.UseCosmos(cosmosCs, Components.Database.Rating);
                    }

                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                    var interceptors = sp.GetServices<IInterceptor>().ToArray();

                    if (interceptors.Length != 0)
                    {
                        options.AddInterceptors(interceptors);
                    }
                }
            );
            builder.EnrichCosmosDbContext<RatingDbContext>();
            services.AddRepositories(typeof(RatingApiMarker));

            if (!string.IsNullOrWhiteSpace(cosmosCs))
            {
                services.AddSingleton(sp =>
                {
                    using var scope = sp.CreateScope();
                    return scope
                        .ServiceProvider.GetRequiredService<RatingDbContext>()
                        .Database.GetCosmosClient();
                });
            }

            // Configure Redis Cache
            builder
                .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
                .WithAzureAuthentication();

            builder.AddCaching();
            services.AddIntegrationEventIdempotency();

            // Configure FluentValidation
            services.AddValidatorsFromAssemblyContaining<RatingApiMarker>(
                includeInternalTypes: true
            );

            services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

            // Configure EventBus first
            builder.AddEventBus<RatingApiMarker>(opts =>
            {
                if (!string.IsNullOrWhiteSpace(cosmosCs))
                {
                    opts.UseCosmosDbPersistence(Components.Database.Rating);
                }

                // Preserve per-book / per-feedback ordering on Kafka so concurrent
                // rating updates for the same book land on a single partition.
                opts.MessagePartitioning.ByPropertyNamed("BookId", "FeedbackId");
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
        }
    }
}
