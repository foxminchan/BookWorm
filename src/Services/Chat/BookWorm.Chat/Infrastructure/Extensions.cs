using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.Chat.Models;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        builder.AddQdrantClient(Components.VectorDb);
        builder.Services.AddQdrantCollection<Guid, ChatHistoryItem>(
            nameof(Chat).ToLowerInvariant()
        );
        builder.AddRedisClient(Components.Redis, o => o.DisableAutoActivation = false);
    }

    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddChatClient();
        builder.AddAgentsTelemetry();
        builder.AddMcpClient(Services.McpTools);

        builder.AddAgents();
        services.AddSingleton<OrchestrateAgents>();
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();
        services.AddScoped<IChatStreaming, ChatStreaming.ChatStreaming>();
    }
}
