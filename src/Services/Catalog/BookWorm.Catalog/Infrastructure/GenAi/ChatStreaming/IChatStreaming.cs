using BookWorm.Catalog.Infrastructure.GenAi.ConversationState.Abstractions;

namespace BookWorm.Catalog.Infrastructure.GenAi.ChatStreaming;

public interface IChatStreaming
{
    Task<Guid> AddStreamingMessage(string text);

    IAsyncEnumerable<ClientMessageFragment> GetMessageStream(
        Guid conversationId,
        Guid? lastMessageId,
        Guid? lastDeliveredFragment,
        CancellationToken cancellationToken = default
    );
}
