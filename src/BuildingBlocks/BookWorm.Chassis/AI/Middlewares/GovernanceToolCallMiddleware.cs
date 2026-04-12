using AgentGovernance;
using BookWorm.Chassis.AI.Governance.AuditTrail;
using BookWorm.Chassis.AI.Governance.Detectors;
using BookWorm.Chassis.AI.Governance.IdentityProvider;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

internal static class GovernanceToolCallMiddleware
{
    public static Func<
        IEnumerable<ChatMessage>,
        ChatOptions?,
        IChatClient,
        CancellationToken,
        Task<ChatResponse>
    > Create(IAgentIdentityProvider identityProvider, string agentName)
    {
        var identity = identityProvider.GetOrCreateIdentity(agentName);

        return async (messages, options, innerChatClient, cancellationToken) =>
        {
            var kernel = innerChatClient.GetRequiredService<GovernanceKernel>();
            var rogueDetector = innerChatClient.GetRequiredService<IRogueAgentDetector>();
            var auditTrail = innerChatClient.GetRequiredService<IGovernanceAuditTrail>();

            // init logger via factory
            var loggerFactory = innerChatClient.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(nameof(GovernanceToolCallMiddleware));

            // Check quarantine status before processing
            if (rogueDetector.IsQuarantined(identity.Did))
            {
                logger?.LogCritical(
                    "Quarantined agent {AgentName} ({Did}) attempted request — rejecting",
                    agentName,
                    identity.Did
                );

                auditTrail.Log(identity.Did, "quarantine_reject", "deny", agentName);

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
            auditTrail.Log(identity.Did, "request_completed", "allow", agentName);

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
                        ctx.AgentDid,
                        tool.Name,
                        new() { ["agent_name"] = ctx.AgentName }
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

    private sealed record GovernanceContext(
        GovernanceKernel Kernel,
        IAgentIdentityProvider IdentityProvider,
        string AgentName,
        string AgentDid,
        IRogueAgentDetector? RogueDetector,
        IGovernanceAuditTrail? AuditTrail,
        ILogger? Logger
    );
}

public static class GovernanceToolCallMiddlewareExtensions
{
    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Adds governance enforcement middleware for tool calls made by the configured agent.
        /// </summary>
        /// <param name="identityProvider">Provides and tracks the agent identity used during governance checks.</param>
        /// <param name="agentName">The logical agent name used to resolve identity and audit entries.</param>
        /// <returns>The same <see cref="ChatClientBuilder" /> instance for fluent middleware configuration.</returns>
        public ChatClientBuilder UseGovernanceToolCall(
            IAgentIdentityProvider identityProvider,
            string agentName
        )
        {
            return builder.Use(
                GovernanceToolCallMiddleware.Create(identityProvider, agentName),
                null
            );
        }
    }
}
