using MassTransit;

namespace BookWorm.Catalog.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        services.AddGrpc();

        services.AddDefaultOpenApi();

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

        services.AddMigration<CatalogDbContext>();

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
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();
        services.AddScoped<IBookRepository, BookRepository>();

        // Configure AI
        services.AddSingleton<IAiService, AiService>();
        builder.AddOllamaApiClient("ollama-nomic-embed-text").AddEmbeddingGenerator();
        builder.AddOllamaApiClient("ollama-deepseek-r1").AddChatClient();
        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter("Experimental.Microsoft.Extensions.AI"))
            .WithTracing(t => t.AddSource("Experimental.Microsoft.Extensions.AI"));

        // Configure Chat AI
        services.AddSignalR();
        services.AddSingleton<IChatStreaming, ChatStreaming>();
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

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

        services.AddAsyncApiDocs([typeof(ICatalogApiMarker)], nameof(Catalog));
    }
}
