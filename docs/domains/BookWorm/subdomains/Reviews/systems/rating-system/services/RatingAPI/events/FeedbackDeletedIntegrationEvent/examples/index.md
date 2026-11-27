<Columns cols={2}>

<Column>

## 1. Subtract a Deleted Review Rating in Catalog

When a feedback record is deleted in **Rating API**, it dispatches `FeedbackDeletedIntegrationEvent` so **Catalog API** decrements the book's `TotalReviews` and recalculates `AverageRating`.

### Payload details

- `bookId`: UUID of the book whose rating summary changes.
- `rating`: Integer rating (`0`–`5`) that was removed.
- `feedbackId`: UUID of the deleted feedback record.

### Example payload

```json
{
	"bookId": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	"rating": 5,
	"feedbackId": "01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01"
}
```

### Using this example

**Catalog API** removes the rating from the book entity and persists the updated `AverageRating` and `TotalReviews` within an idempotent Wolverine handler.

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
[IntegrationMessageOwner(ServiceName = Services.Rating, TopicName = "feedback-delete")]
[MessageIdentity("BookWorm.Contracts.FeedbackDeletedIntegrationEvent")]
public sealed record FeedbackDeletedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;

// 1. Publishing from Rating API via BookWorm.Chassis IEventDispatcher
await eventDispatcher.DispatchAsync(
    new FeedbackDeletedEvent(feedback.BookId, feedback.Rating, feedback.Id),
    cancellationToken
);

// 2. Consuming in Catalog API with Wolverine + IIntegrationEventIdempotencyGuard
internal sealed class FeedbackDeletedIntegrationEventHandler(
    IBookRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(
        FeedbackDeletedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        await idempotencyGuard.ExecuteAsync(
            @event.Id,
            nameof(FeedbackDeletedIntegrationEventHandler),
            async ct =>
            {
                var book = await repository.GetByIdAsync(@event.BookId, ct);
                if (book is not null)
                {
                    book.RemoveRating(@event.Rating);
                    await repository.UnitOfWork.SaveEntitiesAsync(ct);
                }
            },
            cancellationToken: cancellationToken
        );
    }
}
```

</CodeGroup>

</Column>

</Columns>
