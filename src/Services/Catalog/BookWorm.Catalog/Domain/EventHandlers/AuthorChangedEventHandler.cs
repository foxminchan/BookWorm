using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class AuthorChangedEventHandler(IFusionCache cache)
    : INotificationHandler<AuthorCreatedEvent>,
        INotificationHandler<AuthorUpdatedEvent>,
        INotificationHandler<AuthorDeletedEvent>
{
    public async ValueTask Handle(
        AuthorCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CatalogCacheKeys.Authors, token: cancellationToken);
    }

    public async ValueTask Handle(
        AuthorDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    public async ValueTask Handle(
        AuthorUpdatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    private async Task InvalidateWithBooksAsync(CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CatalogCacheKeys.Authors, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookTag, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookListTag, token: cancellationToken);
    }
}
