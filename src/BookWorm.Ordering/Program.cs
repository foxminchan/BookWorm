﻿using BookWorm.Ordering.Grpc;
using BookWorm.Ordering.Infrastructure.Data;
using BookWorm.Ordering.Infrastructure.Redis;
using BookWorm.ServiceDefaults;
using BookWorm.Shared.ActivityScope;
using BookWorm.Shared.Bus;
using BookWorm.Shared.Converters;
using BookWorm.Shared.Endpoints;
using BookWorm.Shared.Exceptions;
using BookWorm.Shared.Identity;
using BookWorm.Shared.Metrics;
using BookWorm.Shared.Pipelines;
using BookWorm.Shared.Versioning;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.Json;
using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;
using GrpcBasketClient = BookWorm.Basket.Grpc.Basket.BasketClient;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

builder.AddRedisCache();

builder.AddVersioning();
builder.AddEndpoints(typeof(Program));

builder.AddOpenApi();

builder.Services.AddSingleton<BookService>();

builder.Services.AddGrpcClient<GrpcBookClient>(o =>
{
    o.Address = new("https+http://catalog-api");
});

builder.Services.AddSingleton<BasketService>();

builder.Services.AddGrpcClient<GrpcBasketClient>(o =>
{
    o.Address = new("https+http://basket-api");
});

builder.Services.AddTransient<IIdentityService, IdentityService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.Run();
