using BookWorm.Chassis.AI.Extensions;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddAIInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();

        services.AddAGUI();
        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
    }
}
