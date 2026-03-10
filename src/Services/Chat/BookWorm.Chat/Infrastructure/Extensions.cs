using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Presidio;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddAIInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();
        builder.AddPresidio();
        builder.AddMcpClient(Services.McpTools);

        services.AddAGUI();
        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
    }
}
