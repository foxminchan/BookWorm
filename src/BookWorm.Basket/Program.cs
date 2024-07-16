using BookWorm.ServiceDefaults;
using BookWorm.Shared.ActivityScope;
using BookWorm.Shared.Endpoints;
using BookWorm.Shared.Exceptions;
using BookWorm.Shared.Metrics;
using BookWorm.Shared.Pipelines;
using BookWorm.Shared.Versioning;
using FluentValidation;

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

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

builder.AddVersioning();
builder.AddEndpoints(typeof(Program));

builder.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.Run();
