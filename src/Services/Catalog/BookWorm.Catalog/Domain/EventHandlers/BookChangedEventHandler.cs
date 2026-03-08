using BookWorm.Catalog.Domain.Events;
using BookWorm.Chassis.Caching;
using Mediator;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class BookChangedEventHandler(IHybridCache cache)
    : INotificationHandler<BookChangedEvent>
{
    public async ValueTask Handle(
        BookChangedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(notification.Key, cancellationToken);
    }
}
