using System.Text;
using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(Http.BuildUrl(Http.Schemes.HttpOrHttps, Services.Rating))
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

                var addressBuilder = new StringBuilder();
                addressBuilder.Append(
                    ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(Services.McpTools)
                );
                addressBuilder.Append('/');
                addressBuilder.Append("mcp");

                var mcpTool = new HostedMcpServerTool(Services.McpTools, addressBuilder.ToString())
                {
                    AllowedTools = ["search_catalog"],
                    ApprovalMode = HostedMcpServerToolApprovalMode.NeverRequire,
                };

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
                            Tools = [mcpTool],
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
                (sp, key) =>
                {
                    var orchestrateAgents = sp.GetRequiredService<OrchestrateAgents>();

                    var handoffWorkflow = AgentWorkflowBuilder
                        .CreateHandoffBuilderWith(orchestrateAgents.RouterAgent)
                        .WithHandoffs(
                            orchestrateAgents.RouterAgent,
                            [
                                orchestrateAgents.LanguageAgent,
                                orchestrateAgents.SummarizeAgent,
                                orchestrateAgents.SentimentAgent,
                                orchestrateAgents.BookAgent,
                            ]
                        )
                        .WithHandoff(
                            orchestrateAgents.LanguageAgent,
                            orchestrateAgents.BookAgent,
                            "Transfer to this agent if the user input is not in English."
                        )
                        .WithHandoff(
                            orchestrateAgents.SummarizeAgent,
                            orchestrateAgents.BookAgent,
                            "Transfer to this agent if the user message is very long or complex."
                        )
                        .WithHandoffs(
                            orchestrateAgents.SentimentAgent,
                            [orchestrateAgents.BookAgent, orchestrateAgents.RouterAgent]
                        )
                        .WithHandoff(
                            orchestrateAgents.BookAgent,
                            orchestrateAgents.RouterAgent,
                            "Transfer back to RouterAgent for any follow-up handling."
                        )
                        .Build();

                    var handoffWorkflowExecutor = handoffWorkflow.BindAsExecutor(
                        "AgentHandoffWorkflowExecutor"
                    );

                    var workflow = new WorkflowBuilder(handoffWorkflowExecutor)
                        .AddEdge(handoffWorkflowExecutor, orchestrateAgents.QAAgent)
                        .WithName(key)
                        .WithDescription(
                            "Orchestrates multiple AI agents to handle user chat messages"
                        )
                        .Build();

                    return workflow;
                }
            )
            .AddAsAIAgent()
            .WithInMemoryThreadStore();
    }

    public static void MapAgentsDiscovery(this WebApplication app)
    {
        app.MapAgentDiscovery("/agents");

        // Map A2A
        app.MapA2A(QAAgent.Name, $"/a2a/{QAAgent.Name}", QAAgent.AgentCard)
            .WithTags(nameof(QAAgent));
        app.MapA2A(RouterAgent.Name, $"/a2a/{RouterAgent.Name}", RouterAgent.AgentCard)
            .WithTags(nameof(RouterAgent));
        app.MapA2A(LanguageAgent.Name, $"/a2a/{LanguageAgent.Name}", LanguageAgent.AgentCard)
            .WithTags(nameof(LanguageAgent));
        app.MapA2A(SummarizeAgent.Name, $"/a2a/{SummarizeAgent.Name}", SummarizeAgent.AgentCard)
            .WithTags(nameof(SummarizeAgent));
        app.MapA2A(SentimentAgent.Name, $"/a2a/{SentimentAgent.Name}", SentimentAgent.AgentCard)
            .WithTags(nameof(SentimentAgent));

        // Map AG-UI
        app.MapAGUI("/ag-ui", app.Services.GetRequiredKeyedService<AIAgent>(Workflows.Chat))
            .WithSummary("Interactive AI Agent")
            .WithTags(nameof(Chat));

        // Map OpenAI Chat Completions
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(SummarizeAgent.Name)
            )
            .WithTags(nameof(SummarizeAgent));
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(LanguageAgent.Name)
            )
            .WithTags(nameof(LanguageAgent));
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(SentimentAgent.Name)
            )
            .WithTags(nameof(SentimentAgent));
    }
}
