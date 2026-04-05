using AgentGovernance;
using BookWorm.Chassis.AI.Governance;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

/// <summary>
///     IChatClient middleware that intercepts tool calls and evaluates them against
///     governance policies before execution. Blocked tools are replaced with a
///     denial message instead of being forwarded to the LLM.
/// </summary>
public static class GovernanceToolCallMiddleware
{
    /// <summary>
    ///     Creates a governance-aware chat client middleware delegate.
    /// </summary>
    /// <param name="kernel">The governance kernel for policy evaluation.</param>
    /// <param name="identityProvider">Provider for agent DID-based identities.</param>
    /// <param name="agentName">The logical name of the agent this middleware protects.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>A delegate compatible with <c>ChatClientBuilder.Use()</c>.</returns>
    public static Func<
        IEnumerable<ChatMessage>,
        ChatOptions?,
        IChatClient,
        CancellationToken,
        Task<ChatResponse>
    > Create(
        GovernanceKernel kernel,
        AgentIdentityProvider identityProvider,
        string agentName,
        ILogger? logger = null
    )
    {
        var identity = identityProvider.GetOrCreateIdentity(agentName);

        return async (messages, options, innerChatClient, cancellationToken) =>
        {
            // Evaluate each tool in the ChatOptions before sending to the LLM
            if (options?.Tools is { Count: > 0 } tools)
            {
                var blockedTools = new List<string>();

                foreach (var tool in tools)
                {
                    var result = kernel.EvaluateToolCall(
                        agentId: identity.Did,
                        toolName: tool.Name,
                        args: new() { ["agent_name"] = agentName }
                    );

                    if (!result.Allowed)
                    {
                        blockedTools.Add(tool.Name);
                        identityProvider.RecordFailure(agentName);

                        logger?.LogWarning(
                            "Governance blocked tool {ToolName} for agent {AgentName}: {Reason}",
                            tool.Name,
                            agentName,
                            result.Reason
                        );
                    }
                }

                // If any tools were blocked, filter them out of the options
                if (blockedTools.Count > 0)
                {
                    var allowedTools = tools.Where(t => !blockedTools.Contains(t.Name)).ToList();

                    options = new()
                    {
                        Instructions = options.Instructions,
                        Temperature = options.Temperature,
                        MaxOutputTokens = options.MaxOutputTokens,
                        TopP = options.TopP,
                        AllowMultipleToolCalls = options.AllowMultipleToolCalls,
                        Tools = allowedTools,
                    };
                }
            }

            // Check for prompt injection in user messages
            if (kernel.InjectionDetector is not null)
            {
                foreach (var message in messages.Where(m => m.Role == ChatRole.User))
                {
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        continue;
                    }

                    var injectionResult = kernel.InjectionDetector.Detect(message.Text);

                    if (injectionResult.IsInjection)
                    {
                        logger?.LogWarning(
                            "Governance detected prompt injection from agent {AgentName}: Type={InjectionType}, Threat={ThreatLevel}",
                            agentName,
                            injectionResult.InjectionType,
                            injectionResult.ThreatLevel
                        );

                        return new([
                            new(
                                ChatRole.Assistant,
                                "I'm unable to process that request due to a security policy violation. Please rephrase your question."
                            ),
                        ]);
                    }
                }
            }

            var response = await innerChatClient.GetResponseAsync(
                messages,
                options,
                cancellationToken
            );

            // Record success for the agent
            identityProvider.RecordSuccess(agentName);

            return response;
        };
    }
}
