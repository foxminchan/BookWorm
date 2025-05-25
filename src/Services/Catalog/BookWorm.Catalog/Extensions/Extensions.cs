using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        services.AddGrpc();

        builder.AddDefaultOpenApi();

        services.AddFeatureManagement();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { new StringTrimmerJsonConverter() } }
        );

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddAzurePostgresDbContext<CatalogDbContext>(
            Components.Database.Catalog,
            app =>
            {
                if (app.Environment.IsDevelopment())
                {
                    services.AddMigration<CatalogDbContext, CatalogDbContextSeed>();
                }
                else
                {
                    services.AddMigration<CatalogDbContext>();
                }

                services.AddRepositories(typeof(ICatalogApiMarker));
            }
        );

        builder.AddQdrantClient(Components.VectorDb);

        builder.AddRedisClient(Components.Redis);

        services.AddHybridCache(options =>
        {
            // Maximum size of cached items
            options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10MB
            options.MaximumKeyLength = 512;

            // Default timeouts
            options.DefaultEntryOptions = new()
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(30),
            };
        });

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ICatalogApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<ICatalogApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure AI
        builder.AddGenAi();

        // Add Blob services
        builder.AddAzureBlobContainerClient(Components.Azure.Storage.BlobContainer);
        services.AddScoped<IBlobService, BlobService>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(ICatalogApiMarker));

        // Other services
        services.AddMapper(typeof(ICatalogApiMarker));

        // Configure EventBus
        builder.AddEventBus(typeof(ICatalogApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(ICatalogApiMarker)]);
    }
}
