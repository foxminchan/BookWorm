using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class CategoryChangedEventHandler(IFusionCache cache)
    : INotificationHandler<CategoryCreatedEvent>,
        INotificationHandler<CategoryUpdatedEvent>,
        INotificationHandler<CategoryDeletedEvent>
{
    public async ValueTask Handle(
        CategoryCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CatalogCacheKeys.Categories, token: cancellationToken);
    }

    public async ValueTask Handle(
        CategoryDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    public async ValueTask Handle(
        CategoryUpdatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    private async Task InvalidateWithBooksAsync(CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CatalogCacheKeys.Categories, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookTag, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookListTag, token: cancellationToken);
    }
}
