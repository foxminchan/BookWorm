using Ardalis.GuardClauses;
using BookWorm.Rating.Domain;
using BookWorm.Rating.IntegrationEvents.Events;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookWorm.Rating.IntegrationEvents.EventHandlers;

public sealed class FeedbackCreatedFailedIntegrationEventHandler(IMongoCollection<Feedback> collection)
    : IConsumer<FeedbackCreatedFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedFailedIntegrationEvent> context)
    {
        var id = ObjectId.Parse(context.Message.FeedbackId);

        var feedback = await collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        Guard.Against.NotFound(id, feedback);

        await collection.DeleteOneAsync(f => f.Id == id);
    }
}
