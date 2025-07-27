using BookWorm.Contracts;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

public sealed class FeedbackCreatedIntegrationEventHandler(IBookRepository repository)
    : IConsumer<FeedbackCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await repository.GetByIdAsync(@event.BookId, context.CancellationToken);

        if (book is null)
        {
            await context.Publish(
                new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId),
                context.CancellationToken
            );
            await repository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
            return;
        }

        book.AddRating(@event.Rating);
        await repository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
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
