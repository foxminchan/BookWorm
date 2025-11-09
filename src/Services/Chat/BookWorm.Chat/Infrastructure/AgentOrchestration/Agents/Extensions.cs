using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new($"{Protocols.HttpOrHttps}://{Services.Rating}")
        );

        builder.AddAIAgent(
            BookAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var mcpClient = sp.GetRequiredService<McpClient>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = BookAgent.Instructions,
                        Description = BookAgent.Description,
                        ChatOptions = new()
                        {
                            Temperature = 0.7f,
                            MaxOutputTokens = 2000,
                            TopP = 0.95f,
                            AllowMultipleToolCalls = true,
                            Tools =
                            [
                                .. mcpClient.ListToolsAsync().Preserve().GetAwaiter().GetResult(),
                                A2AClientFactory
                                    .CreateA2AAgentClient(
                                        Services.Rating,
                                        Constants.Other.Agents.RatingAgent
                                    )
                                    .AsAIFunction(),
                            ],
                        },
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            LanguageAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = LanguageAgent.Instructions,
                        Description = LanguageAgent.Description,
                        ChatOptions = new() { Temperature = 0.3f, MaxOutputTokens = 500 },
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            SentimentAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SentimentAgent.Instructions,
                        Description = SentimentAgent.Description,
                        ChatOptions = new() { Temperature = 0.2f, MaxOutputTokens = 300 },
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            SummarizeAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SummarizeAgent.Instructions,
                        Description = SummarizeAgent.Description,
                        ChatOptions = new() { Temperature = 0.4f, MaxOutputTokens = 800 },
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            QAAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = QAAgent.Instructions,
                        Description = QAAgent.Description,
                        ChatOptions = new() { Temperature = 0.5f, MaxOutputTokens = 1000 },
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            RouterAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = RouterAgent.Instructions,
                        Description = RouterAgent.Description,
                        ChatOptions = new() { Temperature = 0.1f, MaxOutputTokens = 200 },
                    }
                );

                return agent;
            }
        );

        builder
            .AddWorkflow(
                Workflows.Chat,
                (sp, _) =>
                {
                    using var scope = sp.CreateScope();

                    var workflow = scope
                        .ServiceProvider.GetRequiredService<IAgentOrchestrationService>()
                        .BuildAgentsWorkflow();

                    return workflow;
                }
            )
            .AddAsAIAgent()
            .WithInMemoryThreadStore();
    }

    public static void MapAgentsDiscovery(this WebApplication app)
    {
        app.MapAgentDiscovery("/agents");

        app.MapA2A(QAAgent.Name, $"/a2a/{QAAgent.Name}", QAAgent.AgentCard);
        app.MapA2A(RouterAgent.Name, $"/a2a/{RouterAgent.Name}", RouterAgent.AgentCard);
        app.MapA2A(LanguageAgent.Name, $"/a2a/{LanguageAgent.Name}", LanguageAgent.AgentCard);
        app.MapA2A(SummarizeAgent.Name, $"/a2a/{SummarizeAgent.Name}", SummarizeAgent.AgentCard);
        app.MapA2A(SentimentAgent.Name, $"/a2a/{SentimentAgent.Name}", SentimentAgent.AgentCard);

        app.MapAGUI("/ag-ui", app.Services.GetRequiredKeyedService<AIAgent>(Workflows.Chat));
    }
}
