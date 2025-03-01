using BookWorm.Contracts;
using MassTransit;
using Saunter.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class FeedbackDeletedIntegrationEventHandler(IBookRepository repository)
    : IConsumer<FeedbackDeletedIntegrationEvent>
{
    [Channel("catalog-feedback-deleted")]
    [PublishOperation(
        typeof(FeedbackDeletedIntegrationEvent),
        OperationId = nameof(FeedbackDeletedIntegrationEvent),
        Summary = "Update book rating"
    )]
    public async Task Consume(ConsumeContext<FeedbackDeletedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await repository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            return;
        }

        book.RemoveRating(@event.Rating);

        await repository.UnitOfWork.SaveEntitiesAsync();
    }
}

public sealed class FeedbackDeletedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackDeletedIntegrationEventHandler>
{
    public FeedbackDeletedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "catalog-feedback-deleted");
        ConcurrentMessageLimit = 1;
    }
}
