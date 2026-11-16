using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedIntegrationEventHandler(
    IBookRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task<BookUpdatedRatingFailedIntegrationEvent?> Handle(
        FeedbackCreatedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                @event.Id,
                nameof(FeedbackCreatedIntegrationEventHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return null;
        }

        var book = await repository.GetByIdAsync(@event.BookId, cancellationToken);

        if (book is null)
        {
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId);
        }

        book.AddRating(@event.Rating);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return null;
    }
}
