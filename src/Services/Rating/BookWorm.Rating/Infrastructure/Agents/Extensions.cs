using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Rating.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Agents;

public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddChatClient();

        builder.AddEmbeddingGenerator();

        builder.AddAgentsTelemetry();

        services.AddA2AClient(
            Constants.Other.Agents.SummarizeAgent,
            $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Chatting}",
            "a2a"
        );

        services.AddSingleton<ReviewTool>();

        builder.AddAIAgent(
            Constants.Other.Agents.SummarizeAgent,
            (sp, key) =>
            {
                var a2aAgent = sp.GetRequiredService<A2AAgentClient>();

                var agent = a2aAgent.GetAIAgent(key);

                return agent;
            }
        );

        builder.AddAIAgent(
            RatingAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                var reviewPlugin = sp.GetRequiredService<ReviewTool>();

                var agent = new ChatClientAgent(
                    chatClient,
                    name: key,
                    instructions: RatingAgent.Instructions,
                    description: RatingAgent.Description,
                    tools: [.. reviewPlugin.AsAITools()]
                );

                return agent;
            }
        );
    }
}
