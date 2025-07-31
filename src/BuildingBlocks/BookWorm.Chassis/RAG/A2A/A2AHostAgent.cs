using A2A;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chassis.RAG.A2A;

public sealed class A2AHostAgent
{
    private readonly AgentCard _agentCard;

    public A2AHostAgent(Agent agent, AgentCard agentCard, ITaskManager? taskManager = null)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(agentCard);

        Agent = agent;
        _agentCard = agentCard;

        Attach(taskManager ?? new TaskManager());
    }

    private Agent? Agent { get; }

    public ITaskManager? TaskManager { get; private set; }

    public void Attach(ITaskManager taskManager)
    {
        ArgumentNullException.ThrowIfNull(taskManager);

        TaskManager = taskManager;
        taskManager.OnTaskCreated = ExecuteAgentTaskAsync;
        taskManager.OnTaskUpdated = ExecuteAgentTaskAsync;
        taskManager.OnAgentCardQuery = GetAgentCard;
    }

    public async Task ExecuteAgentTaskAsync(
        AgentTask task,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(Agent);

        if (TaskManager is null)
        {
            throw new InvalidOperationException(
                "TaskManager must be attached before executing an agent task."
            );
        }

        await TaskManager
            .UpdateStatusAsync(task.Id, TaskState.Working, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Get message from the user
        var userMessage = task.History![^1].Parts[0].AsTextPart().Text;

        // Get the response from the agent
        var artifact = new Artifact();
        await foreach (
            var response in Agent
                .InvokeAsync(userMessage, cancellationToken: cancellationToken)
                .ConfigureAwait(false)
        )
        {
            var content = response.Message.Content;
            artifact.Parts.Add(new TextPart { Text = content! });
        }

        // Return as artifacts
        await TaskManager
            .ReturnArtifactAsync(task.Id, artifact, cancellationToken)
            .ConfigureAwait(false);

        await TaskManager
            .UpdateStatusAsync(task.Id, TaskState.Completed, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<AgentCard> GetAgentCard(
        string agentUrl,
        CancellationToken cancellationToken = default
    )
    {
        // Ensure the URL is in the correct format
        Uri uri = new(agentUrl);
        agentUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/";

        _agentCard.Url = agentUrl;
        return Task.FromResult(_agentCard);
    }
}
