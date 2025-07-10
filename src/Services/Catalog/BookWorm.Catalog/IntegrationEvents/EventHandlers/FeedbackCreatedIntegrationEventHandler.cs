using BookWorm.Contracts;
using Saunter.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class FeedbackCreatedIntegrationEventHandler(IBookRepository repository)
    : IConsumer<FeedbackCreatedIntegrationEvent>
{
    [Channel("catalog-feedback-created")]
    [PublishOperation(
        typeof(FeedbackCreatedIntegrationEvent),
        OperationId = nameof(FeedbackCreatedIntegrationEvent),
        Summary = "Update book rating",
        Description = "Represents a successful integration event when creating a feedback in the system"
    )]
    public async Task Consume(ConsumeContext<FeedbackCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await repository.GetByIdAsync(@event.BookId, context.CancellationToken);

        if (book is null)
        {
            await PublishFailedEvent(repository, context, @event.FeedbackId);
            return;
        }

        book.AddRating(@event.Rating);
        await repository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
    }

    [Channel("rating-book-updated-rating-failed")]
    [SubscribeOperation(
        typeof(BookUpdatedRatingFailedIntegrationEvent),
        OperationId = nameof(BookUpdatedRatingFailedIntegrationEvent),
        Summary = "Update book rating",
        Description = "Represents a failed integration event when updating a book's rating in the system"
    )]
    private static async Task PublishFailedEvent(
        IBookRepository bookRepository,
        ConsumeContext<FeedbackCreatedIntegrationEvent> context,
        Guid feedbackId
    )
    {
        await context.Publish(
            new BookUpdatedRatingFailedIntegrationEvent(feedbackId),
            context.CancellationToken
        );
        await bookRepository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
    }
}

[ExcludeFromCodeCoverage]
public sealed class FeedbackCreatedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackCreatedIntegrationEventHandler>
{
    public FeedbackCreatedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "catalog-feedback-created");
        ConcurrentMessageLimit = 1;
    }
}
