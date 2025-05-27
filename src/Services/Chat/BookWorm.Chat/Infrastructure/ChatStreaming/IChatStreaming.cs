namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public interface IChatStreaming
{
    Task AddStreamingMessage(Guid conversationId, string text);

    IAsyncEnumerable<ClientMessageFragment> GetMessageStream(
        Guid conversationId,
        Guid? lastMessageId,
        Guid? lastDeliveredFragment,
        CancellationToken cancellationToken = default
    );
}
