using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Ordering.Configurations;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

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
            services.AddEndpoints(typeof(IOrderingApiMarker));
            services.AddDefaultOpenApi(options =>
                options.ApplyOpenApiInfoDefinitions<OrderingAppSettings>()
            );

            // Add event bus configuration
            var postgresCs = builder.Configuration.GetConnectionString(
                Components.Database.Ordering
            );
            builder.AddEventBus(opts =>
            {
                if (!string.IsNullOrWhiteSpace(postgresCs))
                {
                    opts.PersistMessagesWithPostgresql(postgresCs, "wolverine");
                    opts.UseEntityFrameworkCoreTransactions();
                }

                opts.Discovery.IncludeAssembly(typeof(IOrderingApiMarker).Assembly);
            });

            // Configure gRPC
            builder.AddGrpcServices();

            // Configure Redis distributed lock
            builder.AddDistributedLock();
        }
    }
}
