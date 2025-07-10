using BookWorm.Contracts;
using BookWorm.Rating.Domain.Events;
using Saunter.Attributes;

namespace BookWorm.Rating.Domain.EventHandlers;

[AsyncApi]
public sealed class FeedbackDeletedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackDeletedEvent>
{
    [Channel("catalog-feedback-deleted")]
    [SubscribeOperation(
        typeof(FeedbackDeletedIntegrationEvent),
        OperationId = nameof(FeedbackDeletedIntegrationEvent),
        Summary = "Feedback deleted event",
        Description = "Represents a successful integration event when a feedback is deleted"
    )]
    public async Task Handle(FeedbackDeletedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
