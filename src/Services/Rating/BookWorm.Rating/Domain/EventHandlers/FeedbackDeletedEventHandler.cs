using BookWorm.Rating.Domain.Events;
using Mediator;

namespace BookWorm.Rating.Domain.EventHandlers;

internal sealed class FeedbackDeletedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackDeletedEvent>
{
    public async ValueTask Handle(
        FeedbackDeletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
