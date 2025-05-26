using BookWorm.Constants.Aspire;

namespace BookWorm.Chat.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.AddRedisClient(Components.Redis);

        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        builder.AddChatStreamingServices();
    }
}
