using BookWorm.Chat.Infrastructure.Backplane;

namespace BookWorm.Chat.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        services.AddRateLimiting();

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IChatApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

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
        services.AddTransient<ClaimsPrincipal>(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User
        );

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        builder.AddChatStreamingServices();
    }
}
