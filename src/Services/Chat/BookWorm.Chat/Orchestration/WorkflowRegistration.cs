using Azure.Storage.Blobs;
using BookWorm.Chassis.AI.Decisions;
using BookWorm.Chat.Domain.Services;
using BookWorm.Chat.Infrastructure.Decisions;
using BookWorm.Chat.Orchestration.Executors;
using BookWorm.Constants.Other;

namespace BookWorm.Chat.Orchestration;

internal static class WorkflowRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddChatWorkflow()
        {
            builder
                .AddWorkflow(Workflows.Chat, (sp, key) =>
                {
                    var bookAgent = sp.GetRequiredKeyedService<AIAgent>(
                        Constants.Other.Agents.BookAgent
                    );
                    var qaAgent = sp.GetRequiredKeyedService<AIAgent>(
                        Constants.Other.Agents.QAAgent
                    );
                    var ratingAgent = sp.GetRequiredKeyedService<AIAgent>(
                        Constants.Other.Agents.RatingAgent
                    );

                    var validationService = sp.GetRequiredService<PromptValidationService>();
                    var decisionClient = sp.GetRequiredService<IDecisionClient>();
                    var routerLogger = sp.GetService<ILogger<ChatRoutingDecisionService>>();

                    return BuildChatWorkflow(
                        bookAgent,
                        qaAgent,
                        ratingAgent,
                        validationService,
                        decisionClient,
                        routerLogger,
                        key
                    );
                })
                .AddAsAIAgent()
                .WithAzureBlobSessionStore(
                    (sp, _) => sp.GetRequiredService<BlobContainerClient>(),
                    new() { BlobNamePrefix = "ag-ui/" }
                );
        }
    }

    internal static Workflow BuildChatWorkflow(
        AIAgent bookAgent,
        AIAgent qaAgent,
        AIAgent ratingAgent,
        PromptValidationService validationService,
        IDecisionClient decisionClient,
        ILogger<ChatRoutingDecisionService>? routerLogger = null,
        string name = Workflows.Chat
    )
    {
        // Build specialized sub-workflows so domain agents retain inter-agent handoffs
        // without requiring an LLM-based router agent at the entry point.
        var bookWorkflowExecutor = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(bookAgent)
            .WithHandoff(
                bookAgent,
                ratingAgent,
                "Transfer to this agent to summarize a book's reviews or analyze its rating and review-based quality signals."
            )
            .Build()
            .BindAsExecutor("BookWorkflowExecutor");

        var qaWorkflowExecutor = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(qaAgent)
            .WithHandoff(
                qaAgent,
                bookAgent,
                "Transfer to this agent if a customer-service request also needs book information."
            )
            .Build()
            .BindAsExecutor("QAWorkflowExecutor");

        var ratingWorkflowExecutor = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(ratingAgent)
            .Build()
            .BindAsExecutor("RatingWorkflowExecutor");

        // Bind lambda executors for type extraction from validation and routing results
        Func<InputValidationResult, ChatMessage> extractAccepted = r => r.Message;
        var acceptedExtractor = extractAccepted.BindAsExecutor("AcceptedInputExtractor");
        Func<InputValidationResult, string> extractRejected = r => r.Message.Text;
        var rejectedExtractor = extractRejected.BindAsExecutor("RejectedOutputExtractor");

        var bookRouteForwarder = new RouteForwarderExecutor("BookRouteForwarder");
        var qaRouteForwarder = new RouteForwarderExecutor("QARouteForwarder");
        var ratingRouteForwarder = new RouteForwarderExecutor("RatingRouteForwarder");
        Func<RoutingDecision, string> extractDecisionRejection = r =>
            r.RejectionReason ?? ChatRoutingDecisionService.OutOfScopeMessage;
        var decisionRejectedExtractor = extractDecisionRejection.BindAsExecutor(
            "DecisionRejectedOutputExtractor"
        );

        // Create custom executors
        InputValidationExecutor inputValidator = new(validationService);
        DecisionRouterExecutor decisionRouter = new(decisionClient, routerLogger);
        ResponseFormatterExecutor responseFormatter = new();

        // Build workflow with 3-layer architecture:
        // 1. Input Validation - Validates and preprocesses user input
        // 2. Decision Routing - Routes via Microsoft-Decision-1 (IDecisionClient) instead of an LLM
        //    (rejected or out-of-scope/non-English input short-circuits to output)
        // 3. Response Formatting - Ensures consistent, well-formatted responses
        return new WorkflowBuilder(inputValidator)
            // Layer 1: Switch-case on validation result
            .AddSwitch(inputValidator, switchBuilder =>
                switchBuilder
                    .AddCase<InputValidationResult>(
                        result => result is AcceptedInput,
                        acceptedExtractor
                    )
                    .WithDefault(rejectedExtractor)
            )
            // Layer 1→2: Forward accepted input to the decision router
            .AddEdge(acceptedExtractor, decisionRouter)
            // Layer 2: Switch-case on Microsoft-Decision-1 routing decision
            .AddSwitch(decisionRouter, switchBuilder =>
                switchBuilder
                    .AddCase<RoutingDecision>(
                        decision => decision?.Target == RouteTarget.Book,
                        bookRouteForwarder
                    )
                    .AddCase<RoutingDecision>(
                        decision => decision?.Target == RouteTarget.QA,
                        qaRouteForwarder
                    )
                    .AddCase<RoutingDecision>(
                        decision => decision?.Target == RouteTarget.Rating,
                        ratingRouteForwarder
                    )
                    .WithDefault(decisionRejectedExtractor)
            )
            .AddEdge(bookRouteForwarder, bookWorkflowExecutor)
            .AddEdge(qaRouteForwarder, qaWorkflowExecutor)
            .AddEdge(ratingRouteForwarder, ratingWorkflowExecutor)
            // Layer 2→3: Format the selected agent's response
            .AddEdge(bookWorkflowExecutor, responseFormatter)
            .AddEdge(qaWorkflowExecutor, responseFormatter)
            .AddEdge(ratingWorkflowExecutor, responseFormatter)
            // Set terminal output nodes
            .WithOutputFrom(responseFormatter, rejectedExtractor, decisionRejectedExtractor)
            .WithName(name)
            .WithDescription(
                "Workflow with input validation, Microsoft-Decision-1 routing, specialized-agent handoffs, and response formatting"
            )
            .WithOpenTelemetry()
            .Build();
    }

    [SendsMessage(typeof(ChatMessage))]
    [SendsMessage(typeof(TurnToken))]
    private sealed class RouteForwarderExecutor(string id) : Executor<RoutingDecision>(id)
    {
        public override async ValueTask HandleAsync(
            RoutingDecision message,
            IWorkflowContext context,
            CancellationToken cancellationToken = default
        )
        {
            await context
                .SendMessageAsync(message.Message, cancellationToken)
                .ConfigureAwait(false);
            await context
                .SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
