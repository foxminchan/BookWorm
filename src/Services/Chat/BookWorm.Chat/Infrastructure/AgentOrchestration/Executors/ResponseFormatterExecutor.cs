using System.Text.RegularExpressions;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Executors;

internal sealed partial class ResponseFormatterExecutor()
    : Executor<List<ChatMessage>, string>("ResponseFormatterExecutor")
{
    [GeneratedRegex(@"\n{3,}", RegexOptions.Compiled)]
    private static partial Regex ExcessiveNewlinesRegex();

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

        // Trim any leading/trailing whitespace
        content = content.Trim();

        return ValueTask.FromResult(content);
    }
}
