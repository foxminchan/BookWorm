using BookWorm.Contracts;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedIntegrationEventHandler(IBookRepository repository)
{
    public async Task Handle(
        FeedbackCreatedIntegrationEvent @event,
        IMessageContext context,
        CancellationToken cancellationToken
    )
    {
        var book = await repository.GetByIdAsync(@event.BookId, cancellationToken);

        if (book is null)
        {
            await context.PublishAsync(
                new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId)
            );
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return;
        }

        book.AddRating(@event.Rating);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
