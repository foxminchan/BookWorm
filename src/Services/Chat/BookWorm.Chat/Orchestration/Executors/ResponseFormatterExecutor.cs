using System.Text.RegularExpressions;

namespace BookWorm.Chat.Orchestration.Executors;

internal sealed partial class ResponseFormatterExecutor()
    : Executor<List<ChatMessage>, string>("ResponseFormatterExecutor")
{
    [GeneratedRegex(@"\n{3,}", RegexOptions.Compiled)]
    private static partial Regex ExcessiveNewlinesRegex();

    [GeneratedRegex(
        @"(\[system\]|<<SYS>>|<\|im_start\|>system|###\s*System\s*Prompt|\[INST\])[\s\S]{0,200}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex SystemPromptLeakRegex();

    public override ValueTask<string> HandleAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        // Extract the last assistant message
        var lastMessage = messages.LastOrDefault(m => m.Role == ChatRole.Assistant);

        if (lastMessage is null || string.IsNullOrWhiteSpace(lastMessage.Text))
        {
            return ValueTask.FromResult(
                "I apologize, but I couldn't generate a proper response. Could you please try rephrasing your question?"
            );
        }

        var content = lastMessage.Text.Trim();

        // Strip any leaked system prompt fragments from agent output
        content = SystemPromptLeakRegex().Replace(content, string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            return ValueTask.FromResult(
                "I apologize, but I couldn't generate a proper response. Could you please try rephrasing your question?"
            );
        }

        // Ensure proper sentence ending
        if (
            !content.EndsWith('.')
            && !content.EndsWith('!')
            && !content.EndsWith('?')
            && !content.EndsWith('"')
            && !content.EndsWith(')')
        )
        {
            content += ".";
        }

        // Remove excessive newlines (more than 2 consecutive)
        content = ExcessiveNewlinesRegex().Replace(content, "\n\n");

        return ValueTask.FromResult(content.Trim());
    }
}
