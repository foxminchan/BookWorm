using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.Events;

public sealed class ConversationStartedEvent(Guid conversationId, string userScope) : DomainEvent
{
    public Guid ConversationId { get; init; } = conversationId;
    public string UserScope { get; init; } = userScope;
}
