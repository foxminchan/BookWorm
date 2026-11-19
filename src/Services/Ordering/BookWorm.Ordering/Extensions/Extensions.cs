using BookWorm.Chassis.Caching;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Ordering.Configurations;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;

namespace BookWorm.Ordering.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
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

            services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(DecimalJsonConverter.Instance)
            );

            builder.AddRateLimiting();

            builder.AddRedaction();

            builder.AddPersistenceServices();

            // Configure endpoints
            services.AddVersioning();
            services.AddEndpoints(typeof(OrderingApiMarker));
            services.AddDefaultOpenApi(options =>
                options
                    .MapVogenTypesInOrderingApiMarker()
                    .ApplyOpenApiInfoDefinitions<OrderingAppSettings>()
            );

            // Add event bus configuration
            builder.AddEventBus<OrderingApiMarker>(opts =>
            {
                opts.UsePostgresOutbox(builder.Configuration, Components.Database.Ordering);

                // Preserve per-order saga ordering on Kafka by routing all events that
                // expose an OrderId to the same partition.
                opts.MessagePartitioning.ByPropertyNamed("OrderId");
            });

            // Configure gRPC
            builder.AddGrpcServices();

            // Configure Redis distributed lock
            builder.AddDistributedLock();
        }
    }
}
