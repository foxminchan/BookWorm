namespace BookWorm.Catalog.Extensions;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        services.AddGrpc();

        builder.AddDefaultOpenApi();

        services.AddFeatureManagement();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options
                .UseNpgsql(builder.Configuration.GetConnectionString(Components.Database.Catalog))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
        });
        builder.EnrichNpgsqlDbContext<CatalogDbContext>();

        if (builder.Environment.IsDevelopment())
        {
            services.AddMigration<CatalogDbContext, CatalogDbContextSeed>();
        }
        else
        {
            services.AddMigration<CatalogDbContext>();
        }

        builder.AddQdrantClient(Components.VectorDb);

        builder.AddRedisClient(Components.Redis);

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

        // Configure repositories
        services.AddRepositories(typeof(ICatalogApiMarker));

        // Configure AI
        builder.AddOllamaClient();

        // Add Blob services
        builder.AddAzureBlobClient(Components.Blob);
        services.AddSingleton<IBlobService, BlobService>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(ICatalogApiMarker));

        // Other services
        services.AddScoped<IBookSemanticSearch, BookSemanticSearch>();
        services.AddBookDomainToDtoMapper();

        // Configure EventBus
        builder.AddEventBus(typeof(ICatalogApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddAsyncApiDocs([typeof(ICatalogApiMarker)]);
    }
}
