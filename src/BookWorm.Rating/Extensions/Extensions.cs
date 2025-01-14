﻿namespace BookWorm.Rating.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            options.SerializerOptions.Converters.Add(new StringTrimmerJsonConverter());
        });

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.AddDefaultAuthentication();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
        });

        var conn = builder.Configuration.GetConnectionString(ServiceName.Database.Rating);

        builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(conn));

        builder.Services.AddSingleton(provider =>
            provider
                .GetRequiredService<IMongoClient>()
                .GetDatabase(MongoUrl.Create(conn).DatabaseName)
        );

        builder.Services.AddScoped(provider =>
            provider.GetRequiredService<IMongoDatabase>().GetCollection<Feedback>(nameof(Feedback))
        );

        builder.AddRabbitMqEventBus(
            typeof(Program),
            cfg =>
            {
                cfg.AddMongoDbOutbox(o =>
                {
                    o.DisableInboxCleanupService();
                    o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
                    o.DatabaseFactory(provider => provider.GetRequiredService<IMongoDatabase>());
                    o.UseBusOutbox(bo => bo.DisableDeliveryService());
                });
            }
        );

        builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        builder.Services.AddSingleton<IActivityScope, ActivityScope>();
        builder.Services.AddSingleton<CommandHandlerMetrics>();
        builder.Services.AddSingleton<QueryHandlerMetrics>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRatingRepository, RatingRepository>();

        builder.AddVersioning();
        builder.AddEndpoints(typeof(Program));

        builder.AddOpenApi();

        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
