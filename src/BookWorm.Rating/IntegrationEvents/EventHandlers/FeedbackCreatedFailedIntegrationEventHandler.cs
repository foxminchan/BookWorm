using BookWorm.Contracts;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

internal sealed class FeedbackCreatedFailedIntegrationEventHandler(
    IMongoCollection<Feedback> collection,
    ILogger<FeedbackCreatedFailedIntegrationEventHandler> logger) : IConsumer<FeedbackCreatedFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedFailedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] - Rollback feedback with Id: {FeedbackId}",
            nameof(FeedbackCreatedFailedIntegrationEventHandler), @event.FeedbackId);

        var id = ObjectId.Parse(@event.FeedbackId);

        var feedback = await collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        Guard.Against.NotFound(id, feedback);

        await collection.DeleteOneAsync(f => f.Id == id);
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
