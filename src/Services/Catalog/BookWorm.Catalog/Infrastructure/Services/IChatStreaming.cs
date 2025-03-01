namespace BookWorm.Catalog.Infrastructure.Services;

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
