using BookWorm.Catalog.Configurations;
using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.AspNetCore.Authorization;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace BookWorm.Catalog.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddAppSettings<CatalogAppSettings>();

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
                                $"{Services.Catalog}_{Authorization.Actions.Read}",
                                $"{Services.Catalog}_{Authorization.Actions.Write}"
                            );
                    }
                )
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireScope($"{Services.Catalog}_{Authorization.Actions.Read}")
                        .Build()
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
                .ApplyTransactionBehavior<CatalogDbContext>()
                .AddScoped<CreateBookPreProcessor>()
                .AddScoped<UpdateBookPreProcessor>()
                .AddScoped<UpdateBookPostProcessor>();

            builder.AddRateLimiting();

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.Interceptors.Add<GrpcExceptionInterceptor>();
            });

            services.AddGrpcHealthChecks();

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(StringTrimmerJsonConverter.Instance);
                options.SerializerOptions.Converters.Add(DecimalJsonConverter.Instance);
            });

            // Add database configuration
            builder.AddPersistenceServices();

            // Configure FluentValidation
            services.AddValidatorsFromAssemblyContaining<ICatalogApiMarker>(
                includeInternalTypes: true
            );

            services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

            // Configure AI
            builder.AddAI();

            // Configure endpoints
            services.AddVersioning();
            services.AddEndpoints(typeof(ICatalogApiMarker));
            services.AddDefaultOpenApi(options =>
                options.ApplyOpenApiInfoDefinitions<CatalogAppSettings>()
            );

            // Configure Mapper
            services.AddMapper(typeof(ICatalogApiMarker));

            // Configure EventBus
            var postgresCs = builder.Configuration.GetConnectionString(Components.Database.Catalog);
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

                opts.Discovery.IncludeAssembly(typeof(ICatalogApiMarker).Assembly);
                opts.ListenToIntegrationEventsIn(typeof(ICatalogApiMarker).Assembly);
            });

            services.AddKeycloakTokenIntrospection();
        }
    }
}
