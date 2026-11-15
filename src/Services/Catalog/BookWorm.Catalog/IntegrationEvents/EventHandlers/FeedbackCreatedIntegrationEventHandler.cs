using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine;
using Wolverine.Attributes;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedIntegrationEventHandler(
    IBookRepository repository,
    IMessageBus bus,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(
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
            return;
        }

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
