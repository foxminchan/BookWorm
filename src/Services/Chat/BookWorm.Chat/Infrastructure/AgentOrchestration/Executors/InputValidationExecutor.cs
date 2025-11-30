using System.Text.RegularExpressions;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Executors;

internal sealed partial class InputValidationExecutor()
    : Executor<ChatMessage, ChatMessage>("InputValidationExecutor")
{
    private const int MinLength = 1;
    private const int MaxLength = 2000;
    private const int TruncateLength = 1900;

    [GeneratedRegex(
        @"(ignore\s+(previous|all|above)|system\s*:|role\s*:|<\|.*?\|>|###\s*instruction)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex SuspiciousPatternRegex();

    public override ValueTask<ChatMessage> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var content = message.Text.Trim();

        // Check minimum length
        if (content.Length < MinLength)
        {
            return ValueTask.FromResult(
                new ChatMessage(
                    ChatRole.Assistant,
                    "Could you please provide more details? Your message appears to be empty."
                )
            );
        }

        // Detect suspicious patterns (prompt injection attempts)
        var hasSuspiciousPattern = SuspiciousPatternRegex().IsMatch(content);

        // Check maximum length and truncate if needed
        if (content.Length > MaxLength)
        {
            content = content[..TruncateLength] + "... [Message truncated due to length]";
        }

        // Add warning prefix if suspicious pattern detected
        if (hasSuspiciousPattern)
        {
            content = "[⚠️ Unusual pattern detected] " + content;
        }

        return ValueTask.FromResult(new ChatMessage(ChatRole.User, content));
    }
}
