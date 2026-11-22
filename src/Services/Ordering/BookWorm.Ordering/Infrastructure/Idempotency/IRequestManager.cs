namespace BookWorm.Ordering.Infrastructure.Idempotency;

public interface IRequestManager
{
    Task<bool> IsExistAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task CreateAsync(ClientRequest clientRequest, CancellationToken cancellationToken = default);

    Task<bool> TryCreateAsync(
        ClientRequest clientRequest,
        CancellationToken cancellationToken = default
    );

    async Task DeleteAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
