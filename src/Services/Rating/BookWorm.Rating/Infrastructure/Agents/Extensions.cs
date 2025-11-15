using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Constants.Other;
using BookWorm.Rating.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Agents;

public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();

        builder.AddMcpClient(Constants.Aspire.Services.McpTools);

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
        services.AddScoped<ReviewTool>();
        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Chatting}"
            )
        );

        builder.AddAIAgent(
            RatingAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
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

                    var workflow = new WorkflowBuilder(handoffWorkflowExecutor)
                        .AddEdge(handoffWorkflowExecutor, qaAgent)
                        .WithName(key)
                        .WithDescription(
                            "Orchestrates multiple AI agents to summarize and evaluate book ratings"
                        )
                        .Build();

                    return workflow;
                }
            )
            .AddAsAIAgent();
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
