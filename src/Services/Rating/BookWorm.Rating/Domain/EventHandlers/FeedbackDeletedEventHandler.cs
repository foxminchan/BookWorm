using BookWorm.Contracts;
using BookWorm.Rating.Domain.Events;
using Saunter.Attributes;

namespace BookWorm.Rating.Domain.EventHandlers;

[AsyncApi]
public sealed class FeedbackDeletedEventHandler(IEventDispatcher eventDispatcher)
    : INotificationHandler<FeedbackDeletedEvent>
{
    [Channel("catalog-feedback-deleted")]
    [SubscribeOperation(
        typeof(FeedbackDeletedIntegrationEvent),
        OperationId = nameof(FeedbackDeletedIntegrationEvent)
    )]
    public async Task Handle(FeedbackDeletedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
    }
}
