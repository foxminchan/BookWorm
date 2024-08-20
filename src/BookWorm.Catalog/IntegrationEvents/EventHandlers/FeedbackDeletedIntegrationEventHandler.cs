using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Contracts;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackDeletedIntegrationEventHandler(
    IRepository<Book> repository,
    ILogger<FeedbackDeletedIntegrationEventHandler> logger) : IConsumer<FeedbackDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackDeletedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] - Removing rating {Rating} from book {BookId}",
            nameof(FeedbackDeletedIntegrationEventHandler),
            @event.Rating,
            @event.BookId);

        var book = await repository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            return;
        }

        book.RemoveRating(@event.Rating);

        await repository.UpdateAsync(book);
    }
}

internal sealed class FeedbackDeletedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackDeletedIntegrationEventHandler>
{
    public FeedbackDeletedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "feedback-deleted");
        ConcurrentMessageLimit = 1;
    }
}
