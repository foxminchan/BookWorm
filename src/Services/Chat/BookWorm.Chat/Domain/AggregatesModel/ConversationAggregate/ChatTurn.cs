using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class ChatTurn() : Entity
{
    private ChatTurn(UserPrompt? acceptedPrompt, string? rejectionReason)
        : this()
    {
        Id = Guid.CreateVersion7();
        AcceptedPrompt = acceptedPrompt;
        RejectionReason = rejectionReason;
    }

    public UserPrompt? AcceptedPrompt { get; }
    public AssistantResponse? Response { get; private set; }
    public string? RejectionReason { get; }
    public bool IsRejected => RejectionReason is not null;

    internal static ChatTurn CreateAccepted(UserPrompt prompt)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        return new(prompt, null);
    }

    internal static ChatTurn CreateRejected(string reason)
    {
        return string.IsNullOrWhiteSpace(reason)
            ? throw new ChatDomainException("Rejection reason cannot be empty.")
            : new(null, reason);
    }

    internal void AttachResponse(AssistantResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (IsRejected)
        {
            throw new ChatDomainException(
                "Cannot attach an assistant response to a rejected turn."
            );
        }

        if (Response is not null)
        {
            throw new ChatDomainException("This turn already has an assistant response.");
        }

        Response = response;
    }
}
