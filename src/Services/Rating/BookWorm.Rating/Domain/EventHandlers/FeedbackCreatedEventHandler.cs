using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.Domain.EventHandlers;

public sealed class FeedbackCreatedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackCreatedEvent>
{
    public async Task Handle(FeedbackCreatedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
