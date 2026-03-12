using System.Text.Json;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Ordering.Configurations;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Wolverine.EntityFrameworkCore;
using Wolverine.Persistence;
using Wolverine.Postgresql;

namespace BookWorm.Ordering.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddAppSettings<OrderingAppSettings>();

        builder.AddSecurityServices();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCqrsInfrastructure();

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { DecimalJsonConverter.Instance } }
        );

        builder.AddRateLimiting();

        builder.AddRedaction();

        builder.AddPersistenceServices();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IOrderingApiMarker));
        services.AddDefaultOpenApi(options =>
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<OrderingAppSettings>>()
        );

        // Add event bus configuration
        builder.AddEventBus(
            typeof(IOrderingApiMarker),
            options =>
            {
                var connectionString = builder.Configuration.GetRequiredConnectionString(
                    Components.Database.Ordering
                );

                options.PersistMessagesWithPostgresql(connectionString);

                options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

                options.Policies.AutoApplyTransactions();
            }
        );

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure Redis distributed lock
        builder.AddDistributedLock();
    }
}
