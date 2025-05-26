namespace BookWorm.Catalog.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        builder.AddDefaultCors();

        services.AddGrpc();

        builder.AddDefaultOpenApi();

        services.AddFeatureManagement();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { new StringTrimmerJsonConverter() } }
        );

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddPersistenceServices();

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

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(ICatalogApiMarker));

        // Configure Mapper
        services.AddMapper(typeof(ICatalogApiMarker));

        // Configure EventBus
        builder.AddEventBus(typeof(ICatalogApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(ICatalogApiMarker)]);
    }
}
