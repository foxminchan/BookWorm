<Columns cols={2}>

<Column>

## 1. Propagate a New 5-Star Rating to Catalog

After **Rating API** persists a new feedback record, it dispatches `FeedbackCreatedIntegrationEvent` so **Catalog API** increments the book's `AverageRating` and `TotalReviews`.

### Payload details

- `bookId`: UUID of the reviewed book in **Catalog API**.
- `rating`: Integer rating from `0` through `5`.
- `feedbackId`: UUID of the created feedback record in **Rating API**.

### Example payload

```json
{
	"bookId": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	"rating": 5,
	"feedbackId": "01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01"
}
```

### Using this example

**Catalog API** updates the book's rating aggregates transactionally. If the target `bookId` does not exist, **Catalog API** emits `BookUpdatedRatingFailedIntegrationEvent` so **Rating API** compensates by removing the orphaned feedback.

</Column>

<Column>

<CodeGroup dropdown>

```csharp EventBus.cs
using BookWorm.Chassis.EventBus;
using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

// Integration event contract (BookWorm.Contracts)
[IntegrationMessageOwner(ServiceName = Services.Rating, TopicName = "feedback-create")]
[MessageIdentity("BookWorm.Contracts.FeedbackCreatedIntegrationEvent")]
public sealed record FeedbackCreatedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;

// 1. Publishing from Rating API via BookWorm.Chassis IEventDispatcher
await eventDispatcher.DispatchAsync(
    new FeedbackCreatedEvent(feedback.BookId, feedback.Rating, feedback.Id),
    cancellationToken
);

// 2. Consuming in Catalog API with Wolverine + IIntegrationEventIdempotencyGuard
internal sealed class FeedbackCreatedIntegrationEventHandler(
    IBookRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task<BookUpdatedRatingFailedIntegrationEvent?> Handle(
        FeedbackCreatedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        return await idempotencyGuard.ExecuteAsync(
            @event.Id,
            nameof(FeedbackCreatedIntegrationEventHandler),
            async ct =>
            {
                var book = await repository.GetByIdAsync(@event.BookId, ct);
                if (book is null)
                {
                    await repository.UnitOfWork.SaveEntitiesAsync(ct);
                    return new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId);
                }

                book.AddRating(@event.Rating);
                await repository.UnitOfWork.SaveEntitiesAsync(ct);
                return null;
            },
            cancellationToken: cancellationToken
        );
    }
}
```

</CodeGroup>

</Column>

</Columns>
