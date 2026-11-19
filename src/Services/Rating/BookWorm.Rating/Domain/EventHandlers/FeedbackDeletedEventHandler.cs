using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.Domain.EventHandlers;

internal sealed class FeedbackDeletedEventHandler(IEventDispatcher eventDispatcher)
    : INotificationHandler<FeedbackDeletedEvent>
{
    public async ValueTask Handle(
        FeedbackDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
    }
}
