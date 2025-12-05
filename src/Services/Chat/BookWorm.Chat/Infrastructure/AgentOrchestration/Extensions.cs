using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Conditions;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Executors;
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
            client.BaseAddress = new(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Rating)
                    .Build()
            )
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

                var mcpUrl = HttpUtilities
                    .AsUrlBuilder()
                    .WithBase(
                        ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(Services.McpTools)
                    )
                    .WithPath("mcp")
                    .Build();

                var mcpTool = new HostedMcpServerTool(Services.McpTools, mcpUrl)
                {
                    AllowedTools = ["search_catalog"],
                    ApprovalMode = HostedMcpServerToolApprovalMode.NeverRequire,
                };

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = BookAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = BookAgent.Instructions,
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
                        Description = LanguageAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = LanguageAgent.Instructions,
                            Temperature = 0.3f,
                            MaxOutputTokens = 500,
                        },
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
                        Description = SentimentAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = SentimentAgent.Instructions,
                            Temperature = 0.2f,
                            MaxOutputTokens = 300,
                        },
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
                        Description = SummarizeAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = SummarizeAgent.Instructions,
                            Temperature = 0.4f,
                            MaxOutputTokens = 800,
                        },
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
                        Description = QAAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = QAAgent.Instructions,
                            Temperature = 0.5f,
                            MaxOutputTokens = 1000,
                        },
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
                        Description = RouterAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = RouterAgent.Instructions,
                            Temperature = 0.1f,
                            MaxOutputTokens = 200,
                        },
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

                    // Build handoff workflow for dynamic agent routing
                    // RouterAgent analyzes requests and delegates to specialized agents
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
                        // Define handoff paths for agent collaboration
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

                    // Bind handoff workflow as executor for composition with conditional routing
                    var handoffWorkflowExecutor = handoffWorkflow.BindAsExecutor(
                        "AgentHandoffWorkflowExecutor"
                    );

                    // Create custom executors for input validation and response formatting
                    InputValidationExecutor inputValidator = new();
                    ResponseFormatterExecutor responseFormatter = new();

                    // Build workflow with 4-layer architecture:
                    // 1. Input Validation - Validates and preprocesses user input
                    // 2. Agent Handoff - Routes to appropriate specialized agent
                    // 3. Conditional Post-Processing - Intelligent routing based on output analysis
                    // 4. Response Formatting - Ensures consistent, well-formatted responses
                    var workflow = new WorkflowBuilder(inputValidator)
                        // Layer 1→2: Connect input validator to handoff workflow
                        .AddEdge<ChatMessage>(inputValidator, handoffWorkflowExecutor)
                        // Layer 2→3: Route to QAAgent if output contains policy/service-related content
                        .AddEdge<List<ChatMessage>>(
                            handoffWorkflowExecutor,
                            orchestrateAgents.QAAgent,
                            condition: PolicyKeywordCondition.Evaluate
                        )
                        // Layer 2→3: Route to SentimentAgent if negative sentiment is detected
                        .AddEdge<List<ChatMessage>>(
                            handoffWorkflowExecutor,
                            orchestrateAgents.SentimentAgent,
                            condition: NegativeSentimentCondition.Evaluate
                        )
                        // Layer 3→4: Connect all paths to response formatter
                        .AddEdge<List<ChatMessage>>(handoffWorkflowExecutor, responseFormatter)
                        .AddEdge<List<ChatMessage>>(orchestrateAgents.QAAgent, responseFormatter)
                        .AddEdge<List<ChatMessage>>(
                            orchestrateAgents.SentimentAgent,
                            responseFormatter
                        )
                        // Set response formatter as the final output
                        .WithOutputFrom(responseFormatter)
                        .WithName(key)
                        .WithDescription(
                            "Production-grade workflow with input validation, intelligent agent routing, conditional post-processing, and response formatting"
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
