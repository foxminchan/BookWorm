using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class AgentOrchestrationService(
    OrchestrateAgents orchestrateAgents,
    ILogger<AgentOrchestrationService> logger
) : IAgentOrchestrationService
{
    public Workflow BuildAgentsWorkflow()
    {
        var workflow = AgentWorkflowBuilder
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

        return workflow;
    }

    public async Task<IAsyncEnumerable<ChatMessage>> RunWorkflowStreamingAsync(
        string message,
        CancellationToken cancellationToken = default
    )
    {
        var chatMessage = new ChatMessage(ChatRole.User, message);

        await using var run = await InProcessExecution.StreamAsync(
            BuildAgentsWorkflow(),
            chatMessage,
            cancellationToken: cancellationToken
        );
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage> response = [];
        string? lastExecutorId = null;
        await foreach (
            WorkflowEvent evt in run.WatchStreamAsync(cancellationToken).ConfigureAwait(false)
        )
        {
            switch (evt)
            {
                case AgentRunUpdateEvent e
                    when string.IsNullOrEmpty(e.Update.Text) && e.Update.Contents.Count == 0:
                    continue;
                case AgentRunUpdateEvent e:
                {
                    if (e.ExecutorId != lastExecutorId)
                    {
                        lastExecutorId = e.ExecutorId;
                        logger.LogDebug("Switched to agent {AgentName}", e.Update.AuthorName);
                    }

                    logger.LogDebug(
                        "Agent {AgentName} says: {Text}",
                        e.Update.AuthorName,
                        e.Update.Text
                    );

                    if (
                        e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is { } call
                    )
                    {
                        logger.LogDebug(
                            "Calling function '{FunctionName}' with arguments: {Arguments}",
                            call.Name,
                            JsonSerializer.Serialize(call.Arguments)
                        );
                    }

                    break;
                }
                case WorkflowOutputEvent output:
                    response.AddRange(output.As<List<ChatMessage>>()!);
                    break;
            }
        }

        return response.ToAsyncEnumerable();
    }
}
