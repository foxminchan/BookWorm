using BookWorm.Contracts;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedFailedIntegrationEventHandler(
    IRatingRepository repository,
    ILogger<FeedbackCreatedFailedIntegrationEventHandler> logger) : IConsumer<FeedbackCreatedFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedFailedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] - Rollback feedback with Id: {FeedbackId}",
            nameof(FeedbackCreatedFailedIntegrationEventHandler), @event.FeedbackId);

        var id = ObjectId.Parse(@event.FeedbackId);

        var filter = Builders<Feedback>.Filter.Eq(x => x.Id, id);

        var feedback = await repository.GetAsync(filter, context.CancellationToken);

        Guard.Against.NotFound(id, feedback);

        await repository.DeleteAsync(filter, context.CancellationToken);
    }
}

internal sealed class FeedbackCreatedFailedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackCreatedFailedIntegrationEventHandler>
{
    public FeedbackCreatedFailedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "feedback-created-failed");
        ConcurrentMessageLimit = 1;
    }
}
