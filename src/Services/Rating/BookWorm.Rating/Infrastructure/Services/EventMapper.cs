using BookWorm.Contracts;
using BookWorm.Rating.Domain.Events;

namespace BookWorm.Rating.Infrastructure.Services;

public sealed class EventMapper : IEventMapper
{
    public IntegrationEvent MapToIntegrationEvent(DomainEvent @event)
    {
        return @event switch
        {
            FeedbackCreatedEvent feedbackCreatedEvent => new FeedbackCreatedIntegrationEvent(
                feedbackCreatedEvent.BookId,
                feedbackCreatedEvent.Rating,
                feedbackCreatedEvent.FeedbackId
            ),
            FeedbackDeletedEvent feedbackDeletedEvent => new FeedbackDeletedIntegrationEvent(
                feedbackDeletedEvent.BookId,
                feedbackDeletedEvent.Rating,
                feedbackDeletedEvent.FeedbackId
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null),
        };
    }
}
