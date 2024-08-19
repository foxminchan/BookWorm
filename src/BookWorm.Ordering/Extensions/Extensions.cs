using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;
using GrpcBasketClient = BookWorm.Basket.Grpc.Basket.BasketClient;

namespace BookWorm.Ordering.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<JsonOptions>(options =>
            options.SerializerOptions.Converters.Add(new StringTrimmerJsonConverter()));

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

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

        builder.AddRabbitMqEventBus(typeof(Program), cfg =>
            cfg.AddEntityFrameworkOutbox<OrderingContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);

                o.UsePostgres();

                o.UseBusOutbox();
            }));

        builder.Services.AddMarten(_ =>
            {
                var options = new StoreOptions();

                var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "order_state";
                options.Events.DatabaseSchemaName = schemaName;
                options.DatabaseSchemaName = schemaName;

                var conn = builder.Configuration.GetConnectionString("orderingdb");

                Guard.Against.NullOrEmpty(conn);

                options.Connection(conn);

                options.UseSystemTextJsonForSerialization(EnumStorage.AsString);

                options.Projections.Errors.SkipApplyErrors = false;
                options.Projections.Errors.SkipSerializationErrors = false;
                options.Projections.Errors.SkipUnknownEvents = false;

                options.Projections.LiveStreamAggregation<OrderState>();
                options.Projections.Add<OrderProjection>(ProjectionLifecycle.Async);

                return options;
            })
            .OptimizeArtifactWorkflow(TypeLoadMode.Static)
            .UseLightweightSessions()
            .AddAsyncDaemon(DaemonMode.Solo);

        builder.Services.AddSingleton<IActivityScope, ActivityScope>();
        builder.Services.AddSingleton<CommandHandlerMetrics>();
        builder.Services.AddSingleton<QueryHandlerMetrics>();

        builder.AddRedisCache();

        builder.AddVersioning();
        builder.AddEndpoints(typeof(Program));

        builder.AddOpenApi();

        builder.Services.AddSingleton<IBookService, BookService>();

        builder.Services.AddGrpcClient<GrpcBookClient>(o =>
        {
            o.Address = new("https+http://catalog-api");
        });

        builder.Services.AddSingleton<IBasketService, BasketService>();

        builder.Services.AddGrpcClient<GrpcBasketClient>(o =>
        {
            o.Address = new("https+http://basket-api");
        });

        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
