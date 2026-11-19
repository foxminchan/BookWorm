using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.Events;

public sealed class PromptRejectedEvent(Guid conversationId, string reason) : DomainEvent
{
    public Guid ConversationId { get; init; } = conversationId;
    public string Reason { get; init; } = reason;
}
