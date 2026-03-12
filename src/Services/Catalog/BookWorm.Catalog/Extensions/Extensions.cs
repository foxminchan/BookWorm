using BookWorm.Catalog.Configurations;
using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.Authorization;
using Wolverine.EntityFrameworkCore;
using Wolverine.Persistence;
using Wolverine.Postgresql;

namespace BookWorm.Catalog.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
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
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure Mediator
        services
            .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
            .ApplyLoggingBehavior()
            .ApplyActivityBehavior()
            .ApplyValidationBehavior()
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

        services.AddSingleton(_ =>
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(StringTrimmerJsonConverter.Instance);
            options.Converters.Add(DecimalJsonConverter.Instance);
            return options;
        });

        // Add database configuration
        builder.AddPersistenceServices();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<ICatalogApiMarker>(includeInternalTypes: true);

        services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

        // Configure AI
        builder.AddAI();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(ICatalogApiMarker));
        services.AddDefaultOpenApi(options =>
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<CatalogAppSettings>>()
        );

        // Configure Mapper
        services.AddMapper(typeof(ICatalogApiMarker));

        // Configure EventBus
        builder.AddEventBus(
            typeof(ICatalogApiMarker),
            options =>
            {
                var connectionString = builder.Configuration.GetRequiredConnectionString(
                    Components.Database.Catalog
                );

                options.PersistMessagesWithPostgresql(connectionString);

                options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

                options.Policies.AutoApplyTransactions();
            }
        );

        services.AddKeycloakTokenIntrospection();
    }
}
