using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

builder.AddRabbitMqEventBus(typeof(Program), cfg => cfg.AddInMemoryInboxOutbox());

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

builder.AddVersioning();
builder.AddEndpoints(typeof(Program));

builder.AddRedisCache();

builder.AddOpenApi();
builder.AddDefaultAuthentication();
builder.Services.AddSingleton<BookService>();

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<GrpcBookClient>(o =>
{
    o.Address = new("https+http://catalog-api");
});

builder.Services.AddTransient<IIdentityService, IdentityService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.MapGrpcService<BasketService>();

app.Run();
