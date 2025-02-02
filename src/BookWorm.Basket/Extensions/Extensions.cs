﻿using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;

namespace BookWorm.Basket.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
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

        builder.Services.AddValidatorsFromAssemblyContaining<Program>(
            includeInternalTypes: true
        );

        builder.AddRabbitMqEventBus(
            typeof(global::Program),
            cfg =>
            {
                cfg.AddInMemoryInboxOutbox();
            }
        );

        builder.Services.AddSingleton<IActivityScope, ActivityScope>();
        builder.Services.AddSingleton<CommandHandlerMetrics>();
        builder.Services.AddSingleton<QueryHandlerMetrics>();

        builder.AddVersioning();
        builder.AddEndpoints(typeof(Program));

        builder.AddRedisCache();

        builder.AddOpenApi();
        builder.AddDefaultAuthentication();
        builder.Services.AddSingleton<IBookService, BookService>();

        builder.Services.AddGrpc();
        builder
            .Services.AddGrpcClient<GrpcBookClient>(o =>
            {
                o.Address = new("https://catalog-api");
            })
            .AddStandardResilienceHandler();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
