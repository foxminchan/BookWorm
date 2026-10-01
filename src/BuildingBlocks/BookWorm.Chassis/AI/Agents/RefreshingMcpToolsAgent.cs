using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Mcp;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Agents;

public static class RefreshingMcpToolsAgentExtensions
{
    extension(AIAgent agent)
    {
        /// <summary>
        ///     Adds the currently advertised MCP tools to each run of a chat client agent.
        /// </summary>
        /// <param name="mcpClient">The client used to retrieve the server's current tools.</param>
        /// <param name="includeTool">Returns whether a tool advertised by the server is allowed.</param>
        /// <returns>An agent that refreshes its allowed MCP tools before each run.</returns>
        public AIAgent WithRefreshingMcpTools(McpClient mcpClient, Func<string, bool> includeTool)
        {
            ArgumentNullException.ThrowIfNull(agent);
            ArgumentNullException.ThrowIfNull(mcpClient);
            ArgumentNullException.ThrowIfNull(includeTool);

            return new RefreshingMcpToolsAgent(
                agent,
                async cancellationToken =>
                {
                    var tools = await mcpClient
                        .ListAgentToolsWithTasksAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return [.. tools.Where(tool => includeTool(tool.Name))];
                }
            );
        }
    }
}

internal sealed class RefreshingMcpToolsAgent(
    AIAgent innerAgent,
    Func<CancellationToken, Task<AITool[]>> getTools
) : DelegatingAIAgent(innerAgent)
{
    protected override async Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var tools = await getTools(cancellationToken).ConfigureAwait(false);
        var runOptions = CreateRunOptions(options, tools);

        return await InnerAgent
            .RunAsync(messages, session, runOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var tools = await getTools(cancellationToken).ConfigureAwait(false);
        var runOptions = CreateRunOptions(options, tools);

        await foreach (
            var update in InnerAgent
                .RunStreamingAsync(messages, session, runOptions, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            yield return update;
        }
    }

    internal static ChatClientAgentRunOptions CreateRunOptions(
        AgentRunOptions? options,
        AITool[] tools
    )
    {
        ChatClientAgentRunOptions runOptions = options switch
        {
            null => new(new()),
            ChatClientAgentRunOptions chatClientRunOptions => (ChatClientAgentRunOptions)
                chatClientRunOptions.Clone(),
            _ => throw new ArgumentException(
                $"Refreshing MCP tools requires {nameof(ChatClientAgentRunOptions)}.",
                nameof(options)
            ),
        };

        var chatOptions = runOptions.ChatOptions?.Clone() ?? new();
        chatOptions.Tools = [.. chatOptions.Tools ?? [], .. tools];
        runOptions.ChatOptions = chatOptions;

        return runOptions;
    }
}
