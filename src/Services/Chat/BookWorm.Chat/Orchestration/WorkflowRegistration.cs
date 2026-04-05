using AgentGovernance;
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
                    var governanceKernel = sp.GetRequiredService<GovernanceKernel>();
                    InputValidationExecutor inputValidator = new(governanceKernel);
                    ResponseFormatterExecutor responseFormatter = new();

                    // Build workflow with 4-layer architecture:
                    // 1. Input Validation - Validates and preprocesses user input
                    // 2. Agent Handoff - Routes to appropriate specialized agent
                    //    (rejected input short-circuits to output)
                    // 3. Conditional Post-Processing - Intelligent routing based on output analysis
                    // 4. Response Formatting - Ensures consistent, well-formatted responses
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
                                    .AddCase<List<ChatMessage>>(
                                        output =>
                                            output is not null
                                            && NegativeSentimentCondition.Evaluate(output),
                                        sentimentAgent
                                    )
                                    .WithDefault(responseFormatter)
                        )
                        // Layer 3→4: Connect post-processing paths to response formatter
                        .AddEdge(qaAgent, responseFormatter)
                        .AddEdge(sentimentAgent, responseFormatter)
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
}
