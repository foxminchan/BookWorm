namespace BookWorm.Chat.Infrastructure.CancellationManager;

public interface ICancellationManager
{
    CancellationToken GetCancellationToken(Guid id);
    Task CancelAsync(Guid id);
}
