using BookWorm.Catalog.Domain.Events;
using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Catalog.Domain.EventHandlers;

public sealed class BookChangedEventHandler(HybridCache cache)
    : INotificationHandler<BookChangedEvent>
{
    public async Task Handle(BookChangedEvent notification, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(notification.Key, cancellationToken);
    }
}
