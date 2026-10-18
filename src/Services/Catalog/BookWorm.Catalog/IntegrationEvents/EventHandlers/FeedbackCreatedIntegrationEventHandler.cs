using BookWorm.Contracts;
using Wolverine;
using Wolverine.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedIntegrationEventHandler(
    IBookRepository repository,
    IMessageBus bus
)
{
    [WolverineHandler]
    public async Task Handle(
        FeedbackCreatedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var book = await repository.GetByIdAsync(@event.BookId, cancellationToken);

        if (book is null)
        {
            await bus.PublishAsync(new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId));
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return;
        }

        book.AddRating(@event.Rating);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
