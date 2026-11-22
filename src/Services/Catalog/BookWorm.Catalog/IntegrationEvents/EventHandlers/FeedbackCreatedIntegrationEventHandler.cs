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
        return await idempotencyGuard.ExecuteAsync(
            @event.Id,
            nameof(FeedbackCreatedIntegrationEventHandler),
            async ct =>
            {
                var book = await repository.GetByIdAsync(@event.BookId, ct);

                if (book is null)
                {
                    await repository.UnitOfWork.SaveEntitiesAsync(ct);
                    return new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId);
                }

                book.AddRating(@event.Rating);
                await repository.UnitOfWork.SaveEntitiesAsync(ct);
                return null;
            },
            cancellationToken: cancellationToken
        );
    }
}
