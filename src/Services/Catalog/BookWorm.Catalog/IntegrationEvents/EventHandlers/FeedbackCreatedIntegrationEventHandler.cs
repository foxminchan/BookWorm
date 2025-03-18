using BookWorm.Contracts;
using Saunter.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class FeedbackCreatedIntegrationEventHandler(
    IBookRepository bookRepository,
    IPublishEndpoint publishEndpoint
) : IConsumer<FeedbackCreatedIntegrationEvent>
{
    [Channel("catalog-feedback-created")]
    [PublishOperation(
        typeof(FeedbackCreatedIntegrationEvent),
        OperationId = nameof(FeedbackCreatedIntegrationEvent),
        Summary = "Update book rating"
    )]
    public async Task Consume(ConsumeContext<FeedbackCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await bookRepository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            await PublishFailedEvent(@event.FeedbackId);
            return;
        }

        book.AddRating(@event.Rating);

        await bookRepository.UnitOfWork.SaveEntitiesAsync();
    }

    [Channel("rating-book-updated-rating-failed")]
    [SubscribeOperation(
        typeof(BookUpdatedRatingFailedIntegrationEvent),
        OperationId = nameof(BookUpdatedRatingFailedIntegrationEvent)
    )]
    private async Task PublishFailedEvent(Guid feedbackId)
    {
        await publishEndpoint.Publish(new BookUpdatedRatingFailedIntegrationEvent(feedbackId));
    }
}

public sealed class FeedbackCreatedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackCreatedIntegrationEventHandler>
{
    public FeedbackCreatedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "catalog-feedback-created");
        ConcurrentMessageLimit = 1;
    }
}
