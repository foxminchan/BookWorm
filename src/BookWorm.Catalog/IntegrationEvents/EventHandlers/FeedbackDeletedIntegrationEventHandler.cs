using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.Events;
using BookWorm.Core.SharedKernel;
using MassTransit;

namespace BookWorm.Catalog.IntegrationEvents.EventHandlers;

public sealed class FeedbackDeletedIntegrationEventHandler(IRepository<Book> repository)
    : IConsumer<FeedbackDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<FeedbackDeletedIntegrationEvent> context)
    {
        var @event = context.Message;

        var book = await repository.GetByIdAsync(@event.BookId);

        if (book is null)
        {
            return;
        }

        book.RemoveRating(@event.Rating);

        await repository.UpdateAsync(book);
    }
}
