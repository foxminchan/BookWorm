using AgentGovernance;
using BookWorm.Chat.Domain.Services;
using BookWorm.Chat.Infrastructure.Governance;

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

internal sealed class InputValidationExecutor(PromptValidationService validationService)
    : Executor<ChatMessage, InputValidationResult>(nameof(InputValidationExecutor))
{
    public InputValidationExecutor(GovernanceKernel? governanceKernel = null)
        : this(new PromptValidationService(new GovernancePromptSafetyEvaluator(governanceKernel)))
    { }

    public override ValueTask<InputValidationResult> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var outcome = validationService.Validate(message.Text);

        InputValidationResult result = outcome.IsAccepted
            ? new AcceptedInput(new(ChatRole.User, outcome.Prompt!.Value))
            : new RejectedInput(new(ChatRole.Assistant, outcome.RejectionReason!));

        return ValueTask.FromResult(result);
    }
}
