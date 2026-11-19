using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

internal sealed class BookUpdatedRatingFailedIntegrationEventHandler(
    IFeedbackRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(
        BookUpdatedRatingFailedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                @event.Id,
                nameof(BookUpdatedRatingFailedIntegrationEventHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return;
        }

        var feedback = await repository.GetByIdAsync(@event.FeedbackId, cancellationToken);

        if (feedback is null)
        {
            return;
        }

        repository.Delete(feedback);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
