using System.Text.RegularExpressions;
using AgentGovernance;

namespace BookWorm.Chat.Orchestration.Executors;

internal sealed record AcceptedInput(ChatMessage Message);

internal sealed record RejectedInput(ChatMessage Message);

internal union InputValidationResult(AcceptedInput, RejectedInput)
{
    public bool IsAccepted => Value is AcceptedInput;

    public readonly ChatMessage Message =>
        this switch
        {
            AcceptedInput accepted => accepted.Message,
            RejectedInput rejected => rejected.Message,
        };
}

internal sealed partial class InputValidationExecutor(GovernanceKernel? governanceKernel = null)
    : Executor<ChatMessage, InputValidationResult>(nameof(InputValidationExecutor))
{
    private const int MinLength = 1;
    private const int MaxLength = 2000;
    private const int TruncateLength = 1900;

    [GeneratedRegex(
        @"(ignore\s+(previous|all|above)"
            + @"|system\s*:\s"
            + @"|role\s*:\s"
            + @"|<\|.*?\|>"
            + @"|###\s*instruction"
            + @"|\[INST\]"
            + "|<<SYS>>"
            + @"|<\|im_start\|>"
            + @"|\{\s*system_message"
            + @"|<script[\s>]"
            + @"|javascript:\s*"
            + @"|on\w+=\s*[""']"
            + @"|prompt\s*inject"
            + @"|do\s+not\s+follow"
            + @"|disregard\s+(all|your|the|previous)"
            + @"|you\s+are\s+now\s+a"
            + @"|act\s+as\s+(if|a|an)"
            + @"|reveal\s+(your|the|system)\s*(prompt|instruction)"
            + @"|<\/?\s*(div|span|iframe|img|a|body|html|head|form)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex SuspiciousPatternRegex();

    public override ValueTask<InputValidationResult> HandleAsync(
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
            return Rejected(
                "Could you please provide more details? Your message appears to be empty."
            );
        }

        // Fast-path: block obvious prompt injection attempts via regex
        if (SuspiciousPatternRegex().IsMatch(content))
        {
            return Rejected(
                "I'm sorry, but I can't process that request. Please rephrase your question about books or our bookstore services."
            );
        }

        // Deep injection analysis via Agent Governance Toolkit (7 attack types)
        if (governanceKernel?.InjectionDetector is { } detector)
        {
            var injectionResult = detector.Detect(content);

            if (injectionResult.IsInjection)
            {
                return Rejected(
                    $"I'm sorry, but I can't process that request due to a security policy ({injectionResult.InjectionType}). Please rephrase your question about books or our bookstore services."
                );
            }
        }

        // Check maximum length and truncate if needed
        if (content.Length > MaxLength)
        {
            content = content[..TruncateLength] + "... [Message truncated due to length]";
        }

        return ValueTask.FromResult<InputValidationResult>(
            new AcceptedInput(new(ChatRole.User, content))
        );
    }

    private static ValueTask<InputValidationResult> Rejected(string reason)
    {
        return ValueTask.FromResult<InputValidationResult>(
            new RejectedInput(new(ChatRole.Assistant, reason))
        );
    }
}
