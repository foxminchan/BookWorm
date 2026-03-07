namespace BookWorm.Chassis.Caching;

public interface IHybridCache
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    );

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(
        string key,
        T value,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    );
}
