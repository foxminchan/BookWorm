using System.Text.Json;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Ordering.Configurations;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;

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
        services.AddValidationExceptionHandler();
        services.AddNotFoundExceptionHandler();
        services.AddGlobalExceptionHandler();
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
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });

                cfg.AddConfigureEndpointsCallback(
                    (context, _, configurator) =>
                        configurator.UseEntityFrameworkOutbox<OrderingDbContext>(context)
                );
            }
        );

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure Redis distributed lock
        builder.AddDistributedLock();
    }
}
