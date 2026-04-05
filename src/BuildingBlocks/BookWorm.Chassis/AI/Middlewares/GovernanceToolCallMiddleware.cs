using AgentGovernance;
using BookWorm.Chassis.AI.Governance;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

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
            options = ApplyToolGovernance(
                options,
                kernel,
                identity.Did,
                identityProvider,
                agentName,
                logger
            );

            var chatMessages = messages as ChatMessage[] ?? [.. messages];
            var injectionResponse = DetectPromptInjection(chatMessages, kernel, agentName, logger);

            if (injectionResponse is not null)
            {
                return injectionResponse;
            }

            var response = await innerChatClient.GetResponseAsync(
                chatMessages,
                options,
                cancellationToken
            );

            identityProvider.RecordSuccess(agentName);

            return response;
        };
    }

    private static ChatOptions? ApplyToolGovernance(
        ChatOptions? options,
        GovernanceKernel kernel,
        string agentDid,
        AgentIdentityProvider identityProvider,
        string agentName,
        ILogger? logger
    )
    {
        if (options?.Tools is not { Count: > 0 } tools)
        {
            return options;
        }

        var blockedEvaluations = tools
            .Select(tool =>
                (
                    Tool: tool,
                    Result: kernel.EvaluateToolCall(
                        agentId: agentDid,
                        toolName: tool.Name,
                        args: new() { ["agent_name"] = agentName }
                    )
                )
            )
            .Where(x => !x.Result.Allowed)
            .ToList();

        foreach (var (tool, result) in blockedEvaluations)
        {
            identityProvider.RecordFailure(agentName);

            logger?.LogWarning(
                "Governance blocked tool {ToolName} for agent {AgentName}: {Reason}",
                tool.Name,
                agentName,
                result.Reason
            );
        }

        if (blockedEvaluations.Count == 0)
        {
            return options;
        }

        var blockedNames = blockedEvaluations.Select(x => x.Tool.Name).ToHashSet();
        var allowedTools = tools.Where(t => !blockedNames.Contains(t.Name)).ToList();

        return new()
        {
            Instructions = options.Instructions,
            Temperature = options.Temperature,
            MaxOutputTokens = options.MaxOutputTokens,
            TopP = options.TopP,
            AllowMultipleToolCalls = options.AllowMultipleToolCalls,
            Tools = allowedTools,
        };
    }

    private static ChatResponse? DetectPromptInjection(
        IEnumerable<ChatMessage> messages,
        GovernanceKernel kernel,
        string agentName,
        ILogger? logger
    )
    {
        if (kernel.InjectionDetector is not { } detector)
        {
            return null;
        }

        foreach (var message in messages.Where(m => m.Role == ChatRole.User))
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                continue;
            }

            var injectionResult = detector.Detect(message.Text);

            if (!injectionResult.IsInjection)
            {
                continue;
            }

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

        return null;
    }
}
