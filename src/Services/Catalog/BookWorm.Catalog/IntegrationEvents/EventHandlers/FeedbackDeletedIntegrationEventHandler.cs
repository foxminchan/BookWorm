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
        await idempotencyGuard.ExecuteAsync(
            @event.Id,
            nameof(FeedbackDeletedIntegrationEventHandler),
            async ct =>
            {
                var book = await repository.GetByIdAsync(@event.BookId, ct);

                if (book is null)
                {
                    return;
                }

                book.RemoveRating(@event.Rating);

                await repository.UnitOfWork.SaveEntitiesAsync(ct);
            },
            cancellationToken: cancellationToken
        );
    }
}
