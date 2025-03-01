namespace BookWorm.Catalog.Infrastructure.ConversationState.Abstractions;

public interface IConversationState
{
    Task CompleteAsync(Guid conversationId, Guid messageId);
    Task PublishFragmentAsync(Guid conversationId, ClientMessageFragment fragment);

    IAsyncEnumerable<ClientMessageFragment> Subscribe(
        Guid conversationId,
        Guid? lastMessageId,
        CancellationToken cancellationToken = default
    );

    Task<IList<ClientMessage>> GetUnpublishedMessagesAsync(Guid conversationId);
}
