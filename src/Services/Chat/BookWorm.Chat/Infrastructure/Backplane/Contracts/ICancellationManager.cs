namespace BookWorm.Chat.Infrastructure.Backplane.Contracts;

public interface ICancellationManager
{
    CancellationToken GetCancellationToken(Guid id);
    Task CancelAsync(Guid id);
}
