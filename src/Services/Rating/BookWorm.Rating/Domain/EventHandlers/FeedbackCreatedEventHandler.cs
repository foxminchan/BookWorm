using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.Domain.EventHandlers;

internal sealed class FeedbackCreatedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackCreatedEvent>
{
    public async ValueTask Handle(
        FeedbackCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
