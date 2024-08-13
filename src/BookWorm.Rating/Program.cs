using BookWorm.Rating.Domain;
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
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

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
builder.AddMongoDBClient("mongodb");
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
});

builder.AddRabbitMqEventBus(typeof(Program), cfg => cfg.AddInMemoryInboxOutbox());

builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

builder.Services.AddSingleton(serviceProvider =>
{
    var settings = builder.Configuration.GetConnectionString("mongodb");
    var client = serviceProvider.GetService<IMongoClient>();
    return client!.GetDatabase(MongoUrl.Create(settings).DatabaseName);
});

builder.Services.AddScoped(serviceProvider =>
{
    var database = serviceProvider.GetService<IMongoDatabase>();
    return database!.GetCollection<Feedback>("Feedback");
});

builder.AddVersioning();
builder.AddEndpoints(typeof(Program));

builder.AddOpenApi();

builder.Services.AddTransient<IIdentityService, IdentityService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.Run();
