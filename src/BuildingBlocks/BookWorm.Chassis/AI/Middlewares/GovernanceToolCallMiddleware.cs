using AgentGovernance;
using BookWorm.Chassis.AI.Governance;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

public static class GovernanceToolCallMiddleware
{
    private sealed record GovernanceContext(
        GovernanceKernel Kernel,
        AgentIdentityProvider IdentityProvider,
        string AgentName,
        string AgentDid,
        RogueAgentDetector? RogueDetector,
        GovernanceAuditTrail? AuditTrail,
        ILogger? Logger
    );

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

            var ctx = new GovernanceContext(
                kernel,
                identityProvider,
                agentName,
                identity.Did,
                rogueDetector,
                auditTrail,
                logger
            );

            options = ApplyToolGovernance(options, ctx);

            var chatMessages = messages as ChatMessage[] ?? [.. messages];
            var injectionResponse = DetectPromptInjection(chatMessages, ctx);

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

    private static ChatOptions? ApplyToolGovernance(ChatOptions? options, GovernanceContext ctx)
    {
        if (options?.Tools is not { Count: > 0 } tools)
        {
            return options;
        }

        var blockedEvaluations = tools
            .Select(tool =>
                (
                    Tool: tool,
                    Result: ctx.Kernel.EvaluateToolCall(
                        agentId: ctx.AgentDid,
                        toolName: tool.Name,
                        args: new() { ["agent_name"] = ctx.AgentName }
                    )
                )
            )
            .Where(x => !x.Result.Allowed)
            .ToList();

        foreach (var (tool, result) in blockedEvaluations)
        {
            ctx.IdentityProvider.RecordFailure(ctx.AgentName);

            ctx.Logger?.LogWarning(
                "Governance blocked tool {ToolName} for agent {AgentName}: {Reason}",
                tool.Name,
                ctx.AgentName,
                result.Reason
            );

            ctx.AuditTrail?.Log(
                ctx.AgentDid,
                "tool_blocked",
                "deny",
                $"{tool.Name}: {result.Reason}"
            );

            // Record blocked tool call for rogue detection
            ctx.RogueDetector?.RecordCall(ctx.AgentDid, tool.Name);
        }

        // Record allowed tool calls for rogue detection baseline
        var blockedNames = blockedEvaluations.Select(x => x.Tool.Name).ToHashSet();
        var allowedTools = tools.Where(t => !blockedNames.Contains(t.Name)).ToList();

        foreach (var toolName in allowedTools.Select(tool => tool.Name))
        {
            ctx.RogueDetector?.RecordCall(ctx.AgentDid, toolName);
            ctx.AuditTrail?.Log(ctx.AgentDid, "tool_allowed", "allow", toolName);
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
        GovernanceContext ctx
    )
    {
        if (ctx.Kernel.InjectionDetector is not { } detector)
        {
            return null;
        }

        foreach (
            var text in messages
                .Where(m => m.Role == ChatRole.User)
                .Select(message => message.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
        )
        {
            var injectionResult = detector.Detect(text);

            if (!injectionResult.IsInjection)
            {
                continue;
            }

            ctx.Logger?.LogWarning(
                "Governance detected prompt injection from agent {AgentName}: Type={InjectionType}, Threat={ThreatLevel}",
                ctx.AgentName,
                injectionResult.InjectionType,
                injectionResult.ThreatLevel
            );

            ctx.AuditTrail?.Log(
                ctx.AgentName,
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
