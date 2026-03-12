using BookWorm.Contracts;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

public sealed class BookUpdatedRatingFailedIntegrationEventHandler(IFeedbackRepository repository)
{
    public async Task Handle(
        BookUpdatedRatingFailedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var feedback = await repository.GetByIdAsync(@event.FeedbackId, cancellationToken);

        if (feedback is null)
        {
            return;
        }

        repository.Delete(feedback);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
