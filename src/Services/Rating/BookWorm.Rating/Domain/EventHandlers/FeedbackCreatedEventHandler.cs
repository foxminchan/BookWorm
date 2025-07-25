﻿using BookWorm.Contracts;
using BookWorm.Rating.Domain.Events;
using Saunter.Attributes;

namespace BookWorm.Rating.Domain.EventHandlers;

[AsyncApi]
public sealed class FeedbackCreatedEventHandler(
    IEventDispatcher eventDispatcher,
    RatingDbContext dbContext
) : INotificationHandler<FeedbackCreatedEvent>
{
    [Channel("catalog-feedback-created")]
    [SubscribeOperation(
        typeof(FeedbackCreatedIntegrationEvent),
        OperationId = nameof(FeedbackCreatedIntegrationEvent),
        Summary = "Feedback created event",
        Description = "Represents a successful integration event when a feedback is created"
    )]
    public async Task Handle(FeedbackCreatedEvent notification, CancellationToken cancellationToken)
    {
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await dbContext.SaveEntitiesAsync(cancellationToken);
    }
}
