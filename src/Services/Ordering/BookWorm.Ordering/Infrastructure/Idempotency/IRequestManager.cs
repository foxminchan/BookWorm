namespace BookWorm.Ordering.Infrastructure.Idempotency;

internal interface IRequestManager
{
    Task<bool> IsExistAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task CreateAsync(ClientRequest clientRequest, CancellationToken cancellationToken = default);
}
