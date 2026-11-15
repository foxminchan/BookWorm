using BookWorm.Chat.Domain.Services;

namespace BookWorm.Chat.Orchestration.Executors;

internal sealed class ResponseFormatterExecutor(ResponseFormattingService formattingService)
    : Executor<List<ChatMessage>, string>(nameof(ResponseFormatterExecutor))
{
    public ResponseFormatterExecutor()
        : this(new ResponseFormattingService()) { }

    public override ValueTask<string> HandleAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        var lastMessage = messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
        var formatted = formattingService.Format(lastMessage?.Text);

        return ValueTask.FromResult(formatted.Value);
    }
}
