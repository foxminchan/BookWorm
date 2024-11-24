using BookWorm.Ordering.Infrastructure.Marten;
using Microsoft.Extensions.Options;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using GrpcBasketClient = BookWorm.Basket.Grpc.Basket.BasketClient;
using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;

namespace BookWorm.Ordering.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.Services.Configure<JsonOptions>(options =>
            options.SerializerOptions.Converters.Add(new StringTrimmerJsonConverter())
        );

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();

        builder.AddPersistence();
        builder.AddDefaultAuthentication();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        builder.AddRabbitMqEventBus(
            typeof(Program),
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<OrderingContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });
            }
        );

        var martenConfig = builder.Configuration.Get<MartenConfigs>() ?? new MartenConfigs();

        builder.Services.AddNpgsqlDataSource(ServiceName.Database.Ordering);

        builder
            .Services.AddMarten(_ => 
            {
                var options = StoreConfigs.SetStoreOptions(martenConfig);
                options.Projections.LiveStreamAggregation<OrderState>();
                options.Projections.Add<OrderProjection>(ProjectionLifecycle.Async);

            })
            .UseNpgsqlDataSource()
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            .OptimizeArtifactWorkflow(TypeLoadMode.Static)
            .AddAsyncDaemon(martenConfig.DaemonMode);

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(t =>
                t.AddMeter(MartenTelemetry.ActivityName, ActivitySourceProvider.DefaultSourceName)
            )
            .WithTracing(t =>
                t.AddSource(MartenTelemetry.ActivityName, ActivitySourceProvider.DefaultSourceName)
            );

        builder.Services.AddSingleton<IActivityScope, ActivityScope>();
        builder.Services.AddSingleton<CommandHandlerMetrics>();
        builder.Services.AddSingleton<QueryHandlerMetrics>();

        builder.AddRedisCache();

        builder.AddVersioning();
        builder.AddEndpoints(typeof(global::Program));

        builder.AddOpenApi();

        builder.Services.AddSingleton<IBookService, BookService>();

        builder
            .Services.AddGrpcClient<GrpcBookClient>(o =>
            {
                o.Address = new("https://catalog-api");
            })
            .AddStandardResilienceHandler();

        builder.Services.AddSingleton<IBasketService, BasketService>();

        builder
            .Services.AddGrpcClient<GrpcBasketClient>(o =>
            {
                o.Address = new("https://basket-api");
            })
            .AddStandardResilienceHandler();

        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
