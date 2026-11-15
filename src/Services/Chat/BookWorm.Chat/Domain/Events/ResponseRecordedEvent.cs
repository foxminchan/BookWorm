using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.Events;

public sealed class ResponseRecordedEvent(Guid conversationId, string responseText, bool isFallback)
    : DomainEvent
{
    public Guid ConversationId { get; init; } = conversationId;
    public string ResponseText { get; init; } = responseText;
    public bool IsFallback { get; init; } = isFallback;
}
