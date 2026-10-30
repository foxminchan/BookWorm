using AgentGovernance;
using BookWorm.Chat.Agents.BookSearch;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Chat.Agents.Routing;
using BookWorm.Chat.Orchestration.Executors;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Orchestration;

internal static class WorkflowRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddChatWorkflow()
        {
            builder
                .AddWorkflow(
                    Workflows.Chat,
                    (sp, key) =>
                    {
                        var routerAgent = sp.GetRequiredKeyedService<AIAgent>(
                            RouterAgentDefinition.Name
                        );
                        var bookAgent = sp.GetRequiredKeyedService<AIAgent>(
                            BookAgentDefinition.Name
                        );
                        var qaAgent = sp.GetRequiredKeyedService<AIAgent>(QAAgentDefinition.Name);

                        // Build handoff workflow for dynamic agent routing
                        // RouterAgent routes requests to the book or support agent.
                        var handoffWorkflow = AgentWorkflowBuilder
                            .CreateHandoffBuilderWith(routerAgent)
                            .WithHandoffs(routerAgent, [bookAgent, qaAgent])
                            .WithHandoff(
                                qaAgent,
                                bookAgent,
                                "Transfer to this agent if a customer-service request also needs book information."
                            )
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

                        // Build workflow with 3-layer architecture:
                        // 1. Input Validation - Validates and preprocesses user input
                        // 2. Agent Handoff - Routes to appropriate specialized agent
                        //    (rejected input short-circuits to output)
                        // 3. Response Formatting - Ensures consistent, well-formatted responses
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
                            // Layer 2→3: Format the selected agent's response
                            .AddEdge(handoffWorkflowExecutor, responseFormatter)
                            // Set terminal output nodes
                            .WithOutputFrom(responseFormatter, rejectedExtractor)
                            .WithName(key)
                            .WithDescription(
                                "Workflow with input validation, specialized-agent handoffs, and response formatting"
                            )
                            .Build();

                        return workflow;
                    }
                )
                .AddAsAIAgent()
                .WithInMemorySessionStore();
        }
    }
}
