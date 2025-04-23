namespace BookWorm.Catalog.Infrastructure.GenAi.CancellationManager;

public interface ICancellationManager
{
    CancellationToken GetCancellationToken(Guid id);
    Task CancelAsync(Guid id);
}
