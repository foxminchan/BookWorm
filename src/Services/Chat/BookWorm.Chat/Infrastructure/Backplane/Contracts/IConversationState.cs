using BookWorm.Chat.Features;

namespace BookWorm.Chat.Infrastructure.Backplane.Contracts;

public interface IConversationState
{
    /// <summary>
/// Marks the specified message as complete within the given conversation.
/// </summary>
/// <param name="conversationId">The unique identifier of the conversation.</param>
/// <param name="messageId">The unique identifier of the message to mark as complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
Task CompleteAsync(Guid conversationId, Guid messageId);
    /// <summary>
/// Publishes a message fragment to the specified conversation asynchronously.
/// </summary>
/// <param name="conversationId">The unique identifier of the conversation.</param>
/// <param name="fragment">The message fragment to publish.</param>
Task PublishFragmentAsync(Guid conversationId, ClientMessageFragment fragment);

    /// <summary>
    /// Asynchronously streams client message fragments for the specified conversation, starting after the given message ID if provided.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation to subscribe to.</param>
    /// <param name="lastMessageId">The ID of the last message received, or null to start from the beginning.</param>
    /// <param name="cancellationToken">A token to cancel the subscription operation.</param>
    /// <returns>An asynchronous stream of <see cref="ClientMessageFragment"/> objects for the conversation.</returns>
    IAsyncEnumerable<ClientMessageFragment> SubscribeAsync(
        Guid conversationId,
        Guid? lastMessageId,
        CancellationToken cancellationToken = default
    );

    Task<IList<ClientMessage>> GetUnpublishedMessagesAsync(Guid conversationId);
}
