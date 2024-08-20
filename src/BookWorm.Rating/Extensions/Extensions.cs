using BookWorm.Constants;

namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new StringTrimmerJsonConverter());
        });

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.AddDefaultAuthentication();
        builder.AddMongoDBClient(ServiceName.Database.Rating);
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<global::Program>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
        });

        builder.AddRabbitMqEventBus(typeof(global::Program), cfg =>
        {
            cfg.AddInMemoryInboxOutbox();
        });

        builder.Services.AddValidatorsFromAssemblyContaining<global::Program>(includeInternalTypes: true);

        builder.Services.AddSingleton<IActivityScope, ActivityScope>();
        builder.Services.AddSingleton<CommandHandlerMetrics>();
        builder.Services.AddSingleton<QueryHandlerMetrics>();

        builder.Services.AddSingleton(serviceProvider =>
        {
            var url = builder.Configuration.GetConnectionString(ServiceName.Database.Rating);
            var client = serviceProvider.GetService<IMongoClient>();
            return client!.GetDatabase(MongoUrl.Create(url).DatabaseName);
        });

        builder.Services.AddScoped(serviceProvider =>
        {
            var database = serviceProvider.GetService<IMongoDatabase>();
            return database!.GetCollection<Feedback>(nameof(Feedback));
        });

        builder.Services.AddScoped<IRatingRepository, RatingRepository>();

        builder.AddVersioning();
        builder.AddEndpoints(typeof(global::Program));

        builder.AddOpenApi();

        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
