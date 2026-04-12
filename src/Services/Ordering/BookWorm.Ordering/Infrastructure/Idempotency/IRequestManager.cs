namespace BookWorm.Ordering.Infrastructure.Idempotency;

public interface IRequestManager
{
    Task<bool> IsExistAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task CreateAsync(ClientRequest clientRequest, CancellationToken cancellationToken = default);
}
