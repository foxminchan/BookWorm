using A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chassis.RAG.A2A;

public sealed class A2AAgentThread : AgentThread
{
    private readonly A2AClient _client;

    public A2AAgentThread(A2AClient client, string? id = null)
    {
        _client = client;
        Id = id ?? Guid.CreateVersion7().ToString("N");
    }

    protected override Task<string?> CreateInternalAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(Guid.CreateVersion7().ToString("N"));
    }

    protected override Task DeleteInternalAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task OnNewMessageInternalAsync(
        ChatMessageContent newMessage,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }
}
