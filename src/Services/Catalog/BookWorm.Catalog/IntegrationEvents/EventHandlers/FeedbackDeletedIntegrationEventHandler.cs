using BookWorm.Contracts;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackDeletedIntegrationEventHandler(IBookRepository repository)
{
    public async Task Handle(
        FeedbackDeletedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var book = await repository.GetByIdAsync(@event.BookId, cancellationToken);

        if (book is null)
        {
            return;
        }

        book.RemoveRating(@event.Rating);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
