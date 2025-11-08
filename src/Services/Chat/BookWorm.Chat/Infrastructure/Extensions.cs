using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddAIAgentsServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();
        builder.AddMcpClient(Services.McpTools);
        builder.AddAgents();

        services.AddAGUI();
        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
        services.AddSingleton<OrchestrateAgents>();
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();
        services.AddScoped<IChatStreaming, ChatStreaming.ChatStreaming>();
    }
}
