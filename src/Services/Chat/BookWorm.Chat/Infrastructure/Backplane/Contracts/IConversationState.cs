using BookWorm.Chat.Features;

namespace BookWorm.Chat.Infrastructure.Backplane.Contracts;

public interface IConversationState
{
    Task CompleteAsync(Guid conversationId, Guid messageId);
    Task PublishFragmentAsync(Guid conversationId, ClientMessageFragment fragment);

    IAsyncEnumerable<ClientMessageFragment> SubscribeAsync(
        Guid conversationId,
        Guid? lastMessageId,
        CancellationToken cancellationToken = default
    );

    Task<IList<ClientMessage>> GetUnpublishedMessagesAsync(Guid conversationId);
}
