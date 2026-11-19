using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.Events;

public sealed class PromptAcceptedEvent(Guid conversationId, string promptText, bool wasTruncated)
    : DomainEvent
{
    public Guid ConversationId { get; init; } = conversationId;
    public string PromptText { get; init; } = promptText;
    public bool WasTruncated { get; init; } = wasTruncated;
}
