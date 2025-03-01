using BookWorm.Contracts;
using BookWorm.Rating.Domain.Events;
using Saunter.Attributes;

namespace BookWorm.Rating.Domain.EventHandlers;

[AsyncApi]
public sealed class FeedbackCreatedEventHandler(IEventDispatcher eventDispatcher)
    : INotificationHandler<FeedbackCreatedEvent>
{
    [Channel("catalog-feedback-created")]
    [SubscribeOperation(
        typeof(FeedbackCreatedIntegrationEvent),
        OperationId = nameof(FeedbackCreatedIntegrationEvent)
    )]
    public async Task Handle(FeedbackCreatedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
    }
}
