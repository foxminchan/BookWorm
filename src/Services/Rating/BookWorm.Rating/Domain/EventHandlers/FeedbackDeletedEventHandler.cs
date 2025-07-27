using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.Domain.EventHandlers;

public sealed class FeedbackDeletedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackDeletedEvent>
{
    public async Task Handle(FeedbackDeletedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
