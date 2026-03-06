using BookWorm.Chat.Agents.BookSearch;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Chat.Agents.LanguageTranslation;
using BookWorm.Chat.Agents.Routing;
using BookWorm.Chat.Agents.SentimentAnalysis;
using BookWorm.Chat.Agents.Summarization;
using BookWorm.Chat.Orchestration.Conditions;
using BookWorm.Chat.Orchestration.Executors;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Orchestration;

internal static class WorkflowRegistration
{
    public static void AddChatWorkflow(this IHostApplicationBuilder builder)
    {
        builder
            .AddWorkflow(
                Workflows.Chat,
                (sp, key) =>
                {
                    var routerAgent = sp.GetRequiredKeyedService<AIAgent>(
                        RouterAgentDefinition.Name
                    );
                    var bookAgent = sp.GetRequiredKeyedService<AIAgent>(BookAgentDefinition.Name);
                    var languageAgent = sp.GetRequiredKeyedService<AIAgent>(
                        LanguageAgentDefinition.Name
                    );
                    var sentimentAgent = sp.GetRequiredKeyedService<AIAgent>(
                        SentimentAgentDefinition.Name
                    );
                    var summarizeAgent = sp.GetRequiredKeyedService<AIAgent>(
                        SummarizeAgentDefinition.Name
                    );
                    var qaAgent = sp.GetRequiredKeyedService<AIAgent>(QAAgentDefinition.Name);

                    // Build handoff workflow for dynamic agent routing
                    // RouterAgent analyzes requests and delegates to specialized agents
                    var handoffWorkflow = AgentWorkflowBuilder
                        .CreateHandoffBuilderWith(routerAgent)
                        .WithHandoffs(
                            routerAgent,
                            [languageAgent, summarizeAgent, sentimentAgent, bookAgent]
                        )
                        // Define handoff paths for agent collaboration
                        .WithHandoff(
                            languageAgent,
                            bookAgent,
                            "Transfer to this agent if the user input is not in English."
                        )
                        .WithHandoff(
                            summarizeAgent,
                            bookAgent,
                            "Transfer to this agent if the user message is very long or complex."
                        )
                        .WithHandoffs(sentimentAgent, [bookAgent, routerAgent])
                        .WithHandoff(
                            bookAgent,
                            routerAgent,
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
                        .AddEdge(inputValidator, handoffWorkflowExecutor)
                        // Layer 2→3: Route to QAAgent if output contains policy/service-related content
                        .AddEdge<List<ChatMessage>>(
                            handoffWorkflowExecutor,
                            qaAgent,
                            condition: PolicyKeywordCondition.Evaluate
                        )
                        // Layer 2→3: Route to SentimentAgent if negative sentiment is detected
                        .AddEdge<List<ChatMessage>>(
                            handoffWorkflowExecutor,
                            sentimentAgent,
                            condition: NegativeSentimentCondition.Evaluate
                        )
                        // Layer 3→4: Connect all paths to response formatter
                        .AddEdge(handoffWorkflowExecutor, responseFormatter)
                        .AddEdge(qaAgent, responseFormatter)
                        .AddEdge(sentimentAgent, responseFormatter)
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
            .WithInMemorySessionStore();
    }
}
