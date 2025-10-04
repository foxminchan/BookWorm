using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
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

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Chatting}"
            )
        );

        services.AddSingleton<ReviewTool>();

        builder.AddAIAgent(
            Constants.Other.Agents.SummarizeAgent,
            (_, key) => A2AClientExtensions.GetA2AAgent(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            Constants.Other.Agents.LanguageAgent,
            (_, key) => A2AClientExtensions.GetA2AAgent(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            RatingAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build();

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
