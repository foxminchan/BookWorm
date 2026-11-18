using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackDeletedIntegrationEventHandler(
    IBookRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(
        FeedbackDeletedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                @event.Id,
                nameof(FeedbackDeletedIntegrationEventHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return;
        }

        var book = await repository.GetByIdAsync(@event.BookId, cancellationToken);

        if (book is null)
        {
            return;
        }

        book.RemoveRating(@event.Rating);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
