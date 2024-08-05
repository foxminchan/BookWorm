using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.Events;
using BookWorm.Core.SharedKernel;
using MassTransit;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

public sealed class FeedbackCreatedIntegrationEventHandler(
    IRepository<Book> repository,
    IPublishEndpoint publishEndpoint) : IConsumer<FeedbackCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await repository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            await PublishFeedbackCreatedFailed(@event.FeedbackId);
            return;
        }

        book.AddRating(@event.Rating);

        try
        {
            await repository.UpdateAsync(book);
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
