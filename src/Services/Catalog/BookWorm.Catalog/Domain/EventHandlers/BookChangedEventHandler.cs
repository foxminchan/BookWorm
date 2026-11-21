using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class BookChangedEventHandler(IFusionCache cache)
    : INotificationHandler<BookChangedEvent>,
        INotificationHandler<BookCreatedEvent>
{
    public async ValueTask Handle(
        BookChangedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(
            CatalogCacheKeys.BookDetail(notification.BookId),
            token: cancellationToken
        );
        await cache.RemoveAsync(
            CatalogCacheKeys.BookSummary(notification.BookId),
            token: cancellationToken
        );
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookListTag, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookSearchTag, token: cancellationToken);
    }

    public async ValueTask Handle(
        BookCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookListTag, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookSearchTag, token: cancellationToken);
    }
}
