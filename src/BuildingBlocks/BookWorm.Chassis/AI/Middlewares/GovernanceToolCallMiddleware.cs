using AgentGovernance;
using BookWorm.Chassis.AI.Governance;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

public static class GovernanceToolCallMiddleware
{
    /// <summary>
    ///     Creates a governance-aware chat client middleware delegate that enforces policy,
    ///     detects prompt injection, monitors for rogue agent behavior, and logs decisions
    ///     to a tamper-proof Merkle-chained audit trail.
    /// </summary>
    /// <param name="kernel">The governance kernel for policy evaluation.</param>
    /// <param name="identityProvider">Provider for agent DID-based identities.</param>
    /// <param name="agentName">The logical name of the agent this middleware protects.</param>
    /// <param name="rogueDetector">Optional rogue agent detector for anomaly analysis.</param>
    /// <param name="auditTrail">Optional Merkle-chained audit trail for compliance logging.</param>
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
        RogueAgentDetector? rogueDetector = null,
        GovernanceAuditTrail? auditTrail = null,
        ILogger? logger = null
    )
    {
        var identity = identityProvider.GetOrCreateIdentity(agentName);

        return async (messages, options, innerChatClient, cancellationToken) =>
        {
            // Check quarantine status before processing
            if (rogueDetector?.IsQuarantined(identity.Did) is true)
            {
                logger?.LogCritical(
                    "Quarantined agent {AgentName} ({Did}) attempted request — rejecting",
                    agentName,
                    identity.Did
                );

                auditTrail?.Log(identity.Did, "quarantine_reject", "deny", agentName);

                return new([
                    new(
                        ChatRole.Assistant,
                        "This agent has been quarantined due to anomalous behavior. Human review is required before operations can resume."
                    ),
                ]);
            }

            options = ApplyToolGovernance(
                options,
                kernel,
                identity.Did,
                identityProvider,
                agentName,
                rogueDetector,
                auditTrail,
                logger
            );

            var chatMessages = messages as ChatMessage[] ?? [.. messages];
            var injectionResponse = DetectPromptInjection(
                chatMessages,
                kernel,
                agentName,
                auditTrail,
                logger
            );

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
            auditTrail?.Log(identity.Did, "request_completed", "allow", agentName);

            return response;
        };
    }

    private static ChatOptions? ApplyToolGovernance(
        ChatOptions? options,
        GovernanceKernel kernel,
        string agentDid,
        AgentIdentityProvider identityProvider,
        string agentName,
        RogueAgentDetector? rogueDetector,
        GovernanceAuditTrail? auditTrail,
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

            auditTrail?.Log(agentDid, "tool_blocked", "deny", $"{tool.Name}: {result.Reason}");

            // Record blocked tool call for rogue detection
            rogueDetector?.RecordCall(agentDid, tool.Name);
        }

        // Record allowed tool calls for rogue detection baseline
        var blockedNames = blockedEvaluations.Select(x => x.Tool.Name).ToHashSet();
        var allowedTools = tools.Where(t => !blockedNames.Contains(t.Name)).ToList();

        foreach (var tool in allowedTools)
        {
            rogueDetector?.RecordCall(agentDid, tool.Name);
            auditTrail?.Log(agentDid, "tool_allowed", "allow", tool.Name);
        }

        if (blockedEvaluations.Count == 0)
        {
            return options;
        }

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
        GovernanceAuditTrail? auditTrail,
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

            auditTrail?.Log(
                agentName,
                "prompt_injection",
                "deny",
                $"{injectionResult.InjectionType}: {injectionResult.ThreatLevel}"
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
