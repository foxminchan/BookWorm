using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddAIAgentsServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<OrchestrateAgents>();

        builder.AddAIServices().WithAITelemetry();
        builder.AddMcpClient(Services.McpTools);
        builder.AddAgents();

        services.AddAGUI();
        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
    }
}
