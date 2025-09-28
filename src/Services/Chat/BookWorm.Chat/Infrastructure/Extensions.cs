using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.EF;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.Chat.Infrastructure.ChatHistory;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add database configuration
        builder.AddAzureNpgsqlDbContext<ChatDbContext>(Components.Database.Chat);
        services.AddMigration<ChatDbContext>();
        services.AddRepositories(typeof(IChatApiMarker));

        builder.AddRedisClient(Components.Redis, o => o.DisableAutoActivation = false);
    }

    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();
        builder.AddChatCompletion();
        builder.AddMcpClient(Services.McpTools, $"{Protocols.HttpOrHttps}://{Services.McpTools}");
        builder.AddAgents();

        services.AddSingleton<OrchestrateAgents>();
        services.AddScoped<IChatHistoryService, ChatHistoryService>();
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();
        services.AddScoped<IChatStreaming, ChatStreaming.ChatStreaming>();
    }
}
