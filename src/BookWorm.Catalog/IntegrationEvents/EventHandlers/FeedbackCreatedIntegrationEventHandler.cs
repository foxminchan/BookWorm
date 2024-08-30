using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Contracts;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

public sealed class FeedbackCreatedIntegrationEventHandler(
    IRepository<Book> repository,
    ILogger<FeedbackCreatedIntegrationEventHandler> logger,
    IPublishEndpoint publishEndpoint) : IConsumer<FeedbackCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] - Adding rating {Rating} to book {BookId}",
            nameof(FeedbackCreatedIntegrationEventHandler),
            @event.Rating,
            @event.BookId);

        var book = await repository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            await PublishFeedbackCreatedFailed(@event.FeedbackId);
            return;
        }

        book.AddRating(@event.Rating);

        try
        {
            await repository.SaveChangesAsync();
        }
        catch (Exception)
        {
            await PublishFeedbackCreatedFailed(@event.FeedbackId);
        }
    }

    private async Task PublishFeedbackCreatedFailed(string feedbackId)
    {
        await publishEndpoint.Publish(new FeedbackCreatedFailedIntegrationEvent(feedbackId));
    }
}

internal sealed class FeedbackCreatedIntegrationEventHandlerDefinition
    : ConsumerDefinition<FeedbackCreatedIntegrationEventHandler>
{
    public FeedbackCreatedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "feedback-created");
        ConcurrentMessageLimit = 1;
    }
}
