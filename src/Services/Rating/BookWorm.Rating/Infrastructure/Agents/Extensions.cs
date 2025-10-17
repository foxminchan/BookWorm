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

        builder.AddMcpClient(Constants.Aspire.Services.McpTools);

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Chatting}"
            )
        );

        services.AddScoped<ReviewTool>();

        builder.AddAIAgent(
            Constants.Other.Agents.SummarizeAgent,
            (_, key) =>
                A2AClientFactory.CreateA2AAgentClient(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            Constants.Other.Agents.LanguageAgent,
            (_, key) =>
                A2AClientFactory.CreateA2AAgentClient(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            Constants.Other.Agents.SentimentAgent,
            (_, key) =>
                A2AClientFactory.CreateA2AAgentClient(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            Constants.Other.Agents.RouterAgent,
            (_, key) =>
                A2AClientFactory.CreateA2AAgentClient(Constants.Aspire.Services.Chatting, key)
        );

        builder.AddAIAgent(
            RatingAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                using var spScope = sp.CreateScope();
                var reviewPlugin = spScope.ServiceProvider.GetRequiredService<ReviewTool>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = RatingAgent.Instructions,
                        Description = RatingAgent.Description,
                        ChatOptions = new()
                        {
                            Temperature = 0.4f,
                            MaxOutputTokens = 1500,
                            Tools = [.. reviewPlugin.AsAITools()],
                        },
                    }
                );

                return agent;
            }
        );
    }
}
