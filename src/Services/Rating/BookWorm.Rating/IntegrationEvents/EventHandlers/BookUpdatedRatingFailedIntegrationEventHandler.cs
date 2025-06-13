using BookWorm.Contracts;
using Saunter.Attributes;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class BookUpdatedRatingFailedIntegrationEventHandler(IFeedbackRepository repository)
    : IConsumer<BookUpdatedRatingFailedIntegrationEvent>
{
    [Channel("rating-book-updated-rating-failed")]
    [PublishOperation(
        typeof(BookUpdatedRatingFailedIntegrationEvent),
        OperationId = nameof(BookUpdatedRatingFailedIntegrationEvent)
    )]
    public async Task Consume(ConsumeContext<BookUpdatedRatingFailedIntegrationEvent> context)
    {
        var @event = context.Message;

        var feedback = await repository.GetByIdAsync(@event.FeedbackId, context.CancellationToken);

        if (feedback is null)
        {
            return;
        }

        repository.Delete(feedback);

        await repository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
    }
}

[ExcludeFromCodeCoverage]
public sealed class BookUpdatedRatingFailedIntegrationEventHandlerDefinition
    : ConsumerDefinition<BookUpdatedRatingFailedIntegrationEventHandler>
{
    public BookUpdatedRatingFailedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "rating-book-updated-rating-failed");
        ConcurrentMessageLimit = 1;
    }
}
