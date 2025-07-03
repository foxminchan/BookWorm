﻿using BookWorm.Basket.Features.Get;
using BookWorm.Chassis.Mediator;
using MassTransit;

namespace BookWorm.Basket.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddRateLimiting();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddRedisClient(Components.Redis);
        services.AddSingleton<IBasketRepository, BasketRepository>();

        // Configure MediatR
        services.AddMediatR<IBasketApiMarker>(configuration =>
        {
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddRequestPostProcessor<GetBasketPostProcessor>();
        });

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IBasketApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IBasketApiMarker));

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure ClaimsPrincipal
        services.AddTransient<ClaimsPrincipal>(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User
        );

        // Configure EventBus
        builder.AddEventBus(typeof(IBasketApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddDefaultAsyncApi([typeof(IBasketApiMarker)]);
    }
}
