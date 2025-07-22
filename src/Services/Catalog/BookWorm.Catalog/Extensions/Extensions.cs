using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Chassis.Mediator;

namespace BookWorm.Catalog.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        builder.AddDefaultCors();

        services.AddRateLimiting();

        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });
        services.AddGrpcHealthChecks();

        builder.AddDefaultOpenApi();

        services.AddFeatureManagement();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { new StringTrimmerJsonConverter() } }
        );

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddPersistenceServices();

        // Configure MediatR
        services.AddMediatR<ICatalogApiMarker>(configuration =>
        {
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddRequestPreProcessor<CreateBookPreProcessor>();
            configuration.AddRequestPreProcessor<UpdateBookPreProcessor>();
            configuration.AddRequestPostProcessor<UpdateBookPostProcessor>();
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
        builder.AddEventBus(
            typeof(ICatalogApiMarker),
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });

                cfg.AddConfigureEndpointsCallback(
                    (context, _, configurator) =>
                        configurator.UseEntityFrameworkOutbox<CatalogDbContext>(context)
                );
            }
        );

        builder.AddDefaultAsyncApi([typeof(ICatalogApiMarker)]);

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
