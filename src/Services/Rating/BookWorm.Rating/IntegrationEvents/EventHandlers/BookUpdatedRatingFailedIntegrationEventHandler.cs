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
        await idempotencyGuard.ExecuteAsync(
            @event.Id,
            nameof(BookUpdatedRatingFailedIntegrationEventHandler),
            async ct =>
            {
                var feedback = await repository.GetByIdAsync(@event.FeedbackId, ct);

                if (feedback is null)
                {
                    return;
                }

                feedback.Revert();

                repository.Delete(feedback);

                await repository.UnitOfWork.SaveEntitiesAsync(ct);
            },
            cancellationToken: cancellationToken
        );
    }
}
