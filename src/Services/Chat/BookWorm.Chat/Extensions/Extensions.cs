using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Mediator;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Chat.Infrastructure.Backplane;

namespace BookWorm.Chat.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        services.AddRateLimiting();

        builder.AddDefaultCors();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        builder.AddDefaultOpenApi();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure MediatR
        services.AddMediatR<IChatApiMarker>(configuration =>
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>))
        );

        services.AddVersioning();
        services.AddEndpoints(typeof(IChatApiMarker));

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IChatApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        builder.AddPersistenceServices();

        services.AddBackplaneServices();

        // Configure ClaimsPrincipal
        services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        builder.AddChatStreamingServices();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
