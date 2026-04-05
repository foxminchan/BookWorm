using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using BookWorm.Constants.Other;
using BookWorm.Rating.Infrastructure.Agents.Conditions;
using BookWorm.Rating.Infrastructure.Agents.Executors;
using BookWorm.Rating.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();
        builder.AddPresidio();

        builder.AddMcpClient(Constants.Aspire.Services.McpTools);

        builder.AddAgentGovernance("policies/rating-agent.yaml");

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
        services.AddScoped<ReviewTool>();
        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Constants.Aspire.Services.Chatting)
                    .Build()
            )
        );

        builder.AddAIAgent(
            RatingAgent.Name,
            (sp, key) =>
            {
                var presidioService = sp.GetRequiredService<IPresidioService>();
                var governanceKernel = sp.GetRequiredService<AgentGovernance.GovernanceKernel>();
                var identityProvider = sp.GetRequiredService<AgentIdentityProvider>();
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.Create(presidioService), null)
                    .Use(
                        GovernanceToolCallMiddleware.Create(
                            governanceKernel,
                            identityProvider,
                            RatingAgent.Name
                        ),
                        null
                    )
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                using var spScope = sp.CreateScope();
                var reviewPlugin = spScope.ServiceProvider.GetRequiredService<ReviewTool>();
                var mcpClient = sp.GetRequiredService<McpClient>();

                var mcpTools = mcpClient
                    .ListToolsAsync()
                    .Preserve()
                    .GetAwaiter()
                    .GetResult()
                    .Where(t => t.Name == "get_book");

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = RatingAgent.Description,
                        ChatOptions = new()
                        {
                            Instructions = RatingAgent.Instructions,
                            Temperature = 0.4f,
                            MaxOutputTokens = 1500,
                            AllowMultipleToolCalls = true,
                            Tools = [.. reviewPlugin.AsAITools(), .. mcpTools],
                        },
                    }
                );

                return agent;
            }
        );

        builder
            .AddWorkflow(
                Workflows.RatingSummarizer,
                (sp, key) =>
                {
                    var summarizeAgent = A2AClientFactory.CreateA2AAgentClient(
                        sp,
                        Constants.Aspire.Services.Chatting,
                        Constants.Other.Agents.SummarizeAgent
                    );

                    var languageAgent = A2AClientFactory.CreateA2AAgentClient(
                        sp,
                        Constants.Aspire.Services.Chatting,
                        Constants.Other.Agents.LanguageAgent
                    );

                    var sentimentAgent = A2AClientFactory.CreateA2AAgentClient(
                        sp,
                        Constants.Aspire.Services.Chatting,
                        Constants.Other.Agents.SentimentAgent
                    );

                    var routerAgent = A2AClientFactory.CreateA2AAgentClient(
                        sp,
                        Constants.Aspire.Services.Chatting,
                        Constants.Other.Agents.RouterAgent
                    );

                    var qaAgent = A2AClientFactory.CreateA2AAgentClient(
                        sp,
                        Constants.Aspire.Services.Chatting,
                        Constants.Other.Agents.QAAgent
                    );

                    var ratingAgent = sp.GetRequiredKeyedService<AIAgent>(RatingAgent.Name);

                    var handoffWorkflow = AgentWorkflowBuilder
                        .CreateHandoffBuilderWith(routerAgent)
                        .WithHandoffs(
                            routerAgent,
                            [languageAgent, summarizeAgent, sentimentAgent, ratingAgent]
                        )
                        .WithHandoff(
                            languageAgent,
                            ratingAgent,
                            "Transfer to this agent if the user input is not in English."
                        )
                        .WithHandoff(
                            summarizeAgent,
                            ratingAgent,
                            "Transfer to this agent if the user message is very long or complex."
                        )
                        .WithHandoff(
                            sentimentAgent,
                            ratingAgent,
                            "Transfer to this agent if the user message contains emotional content."
                        )
                        .WithHandoff(
                            ratingAgent,
                            routerAgent,
                            "Transfer back to RouterAgent for any follow-up handling."
                        )
                        .Build();

                    var handoffWorkflowExecutor = handoffWorkflow.BindAsExecutor(
                        "RatingSummarizerWorkflowExecutor"
                    );

                    // Bind lambda executors for type extraction from validation result
                    Func<InputValidationResult, ChatMessage> extractAccepted = r => r.Message;
                    var acceptedExtractor = extractAccepted.BindAsExecutor(
                        "AcceptedInputExtractor"
                    );
                    Func<InputValidationResult, string> extractRejected = r => r.Message.Text;
                    var rejectedExtractor = extractRejected.BindAsExecutor(
                        "RejectedOutputExtractor"
                    );

                    // Create custom executors
                    var governanceKernel =
                        sp.GetRequiredService<AgentGovernance.GovernanceKernel>();
                    InputValidationExecutor inputValidator = new(governanceKernel);
                    ResponseFormatterExecutor responseFormatter = new();

                    // Build workflow with 4-layer architecture:
                    // 1. Input Validation — validates and preprocesses user input
                    // 2. Agent Handoff — routes to appropriate specialized agent
                    //    (rejected input short-circuits to output)
                    // 3. Conditional Post-Processing — QA routing for policy-related content
                    // 4. Response Formatting — ensures consistent, sanitized output
                    var workflow = new WorkflowBuilder(inputValidator)
                        // Layer 1: Switch-case on validation result
                        .AddSwitch(
                            inputValidator,
                            switchBuilder =>
                                switchBuilder
                                    .AddCase<InputValidationResult>(
                                        result => result is { IsAccepted: true },
                                        acceptedExtractor
                                    )
                                    .WithDefault(rejectedExtractor)
                        )
                        // Layer 1→2: Forward accepted input to the agent handoff
                        .AddEdge(acceptedExtractor, handoffWorkflowExecutor)
                        // Layer 2→3/4: Switch-case on post-processing conditions
                        .AddSwitch(
                            handoffWorkflowExecutor,
                            switchBuilder =>
                                switchBuilder
                                    .AddCase<List<ChatMessage>>(
                                        output =>
                                            output is not null
                                            && PolicyKeywordCondition.Evaluate(output),
                                        qaAgent
                                    )
                                    .WithDefault(responseFormatter)
                        )
                        // Layer 3→4: Connect QA post-processing to response formatter
                        .AddEdge(qaAgent, responseFormatter)
                        // Set terminal output nodes
                        .WithOutputFrom(responseFormatter, rejectedExtractor)
                        .WithName(key)
                        .WithDescription(
                            "Production-grade workflow with input validation, intelligent agent routing, conditional post-processing, and response formatting"
                        )
                        .Build();

                    return workflow;
                }
            )
            .AddAsAIAgent()
            .WithInMemorySessionStore();
    }

    public static void MapAgentsDiscovery(this WebApplication app)
    {
        app.MapAgentDiscovery("/agents");

        app.MapAGUI(
                "/ag-ui",
                app.Services.GetRequiredKeyedService<AIAgent>(Workflows.RatingSummarizer)
            )
            .WithSummary("Interactive AI Agent")
            .WithTags(nameof(RatingAgent));
    }
}
