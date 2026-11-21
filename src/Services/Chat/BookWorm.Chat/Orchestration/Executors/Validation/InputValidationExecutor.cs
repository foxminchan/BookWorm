using AgentGovernance;
using BookWorm.Chat.Domain.Services;
using BookWorm.Chat.Infrastructure.Governance;

namespace BookWorm.Chat.Orchestration.Executors.Validation;

internal sealed record AcceptedInput(
    ChatMessage Message,
    IReadOnlyList<ChatMessage>? Messages = null
)
{
    public IReadOnlyList<ChatMessage> EffectiveMessages =>
        Messages is { Count: > 0 } ? Messages : [Message];
}

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

internal sealed class InputValidationExecutor(
    PromptValidationService validationService,
    bool ignoreEmptyTurns = false
)
    : ChatProtocolExecutor(
        nameof(InputValidationExecutor),
        new() { StringMessageChatRole = ChatRole.User, AutoSendTurnToken = false },
        true
    )
{
    public InputValidationExecutor(GovernanceKernel? governanceKernel = null)
        : this(new PromptValidationService(new GovernancePromptSafetyEvaluator(governanceKernel)))
    { }

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        return base.ConfigureProtocol(protocolBuilder).SendsMessage<InputValidationResult>();
    }

    public ValueTask<InputValidationResult> HandleAsync(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return ValueTask.FromResult(ValidateMessages([message]));
    }

    protected override async ValueTask TakeTurnAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        bool? emitEvents,
        CancellationToken cancellationToken = default
    )
    {
        if (IsPureToolResultResumeTurn(messages) || (ignoreEmptyTurns && messages.Count == 0))
        {
            return;
        }

        var result = ValidateMessages(messages);
        await context.SendMessageAsync(result, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsPureToolResultResumeTurn(IReadOnlyList<ChatMessage> messages)
    {
        return messages.Count > 0
            && messages[^1].Role == ChatRole.Tool
            && messages[^1].Contents.OfType<FunctionResultContent>().Any();
    }

    private InputValidationResult ValidateMessages(IReadOnlyList<ChatMessage> messages)
    {
        if (messages.Count == 0)
        {
            var emptyOutcome = validationService.Validate(string.Empty);
            return new RejectedInput(new(ChatRole.Assistant, emptyOutcome.RejectionReason!));
        }

        var targetIndex = FindLastUserMessageIndex(messages);
        var targetMessage = messages[targetIndex];
        var outcome = validationService.Validate(targetMessage.Text);

        if (!outcome.IsAccepted)
        {
            return new RejectedInput(new(ChatRole.Assistant, outcome.RejectionReason!));
        }

        var sanitizedMessage = new ChatMessage(ChatRole.User, outcome.Prompt!.Value);
        List<ChatMessage> updatedMessages = [.. messages];
        updatedMessages[targetIndex] = sanitizedMessage;

        return new AcceptedInput(sanitizedMessage, updatedMessages);
    }

    private static int FindLastUserMessageIndex(IReadOnlyList<ChatMessage> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i].Role == ChatRole.User)
            {
                return i;
            }
        }

        return messages.Count - 1;
    }
}
