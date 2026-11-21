using BookWorm.Catalog.Domain.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class PublisherChangedEventHandler(IFusionCache cache)
    : INotificationHandler<PublisherCreatedEvent>,
        INotificationHandler<PublisherUpdatedEvent>,
        INotificationHandler<PublisherDeletedEvent>
{
    public async ValueTask Handle(
        PublisherCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CatalogCacheKeys.Publishers, token: cancellationToken);
    }

    public async ValueTask Handle(
        PublisherDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    public async ValueTask Handle(
        PublisherUpdatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await InvalidateWithBooksAsync(cancellationToken);
    }

    private async Task InvalidateWithBooksAsync(CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CatalogCacheKeys.Publishers, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookTag, token: cancellationToken);
        await cache.RemoveByTagAsync(CatalogCacheKeys.BookListTag, token: cancellationToken);
    }
}
