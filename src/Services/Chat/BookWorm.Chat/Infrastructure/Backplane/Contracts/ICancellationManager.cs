namespace BookWorm.Chat.Infrastructure.Backplane.Contracts;

public interface ICancellationManager
{
    CancellationToken GetCancellationToken(Guid conversationId, Guid messageId);
    Task CancelAsync(Guid conversationId);
}
