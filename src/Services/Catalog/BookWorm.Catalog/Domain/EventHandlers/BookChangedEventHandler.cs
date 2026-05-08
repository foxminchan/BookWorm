using BookWorm.Catalog.Domain.Events;
using Mediator;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Domain.EventHandlers;

internal sealed class BookChangedEventHandler(IFusionCache cache)
    : INotificationHandler<BookChangedEvent>
{
    public async ValueTask Handle(
        BookChangedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(notification.Key, token: cancellationToken);
    }
}
