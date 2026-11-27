<Columns cols={2}>

<Column>

## 1. Compensate Orphaned Feedback When Book Is Missing in Catalog

When **Catalog API** receives `FeedbackCreatedIntegrationEvent` for a `bookId` that does not exist (or was deleted), `FeedbackCreatedIntegrationEventHandler` returns `BookUpdatedRatingFailedIntegrationEvent` so **Rating API** deletes the orphaned feedback record.

### Payload details

- `feedbackId`: UUID of the feedback record in **Rating API** that could not be applied.

### Example payload

```json
{
	"feedbackId": "01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01"
}
```

### Using this example

**Rating API** idempotently removes the orphaned feedback record so review counts and catalog book state remain consistent.

</Column>

<Column>

<CodeGroup dropdown>

```csharp EventBus.cs
using BookWorm.Chassis.EventBus;
using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

// Integration event contract (BookWorm.Contracts)
[IntegrationMessageOwner(ServiceName = Services.Catalog, TopicName = "book-rating-failed")]
[MessageIdentity("BookWorm.Contracts.BookUpdatedRatingFailedIntegrationEvent")]
public sealed record BookUpdatedRatingFailedIntegrationEvent(Guid FeedbackId)
    : IntegrationEvent;

// 1. Emitted by Catalog API when the target book is not found
if (book is null)
{
    await repository.UnitOfWork.SaveEntitiesAsync(ct);
    return new BookUpdatedRatingFailedIntegrationEvent(@event.FeedbackId);
}

// 2. Consumed in Rating API with Wolverine + IIntegrationEventIdempotencyGuard
internal sealed class BookUpdatedRatingFailedIntegrationEventHandler(
    IFeedbackRepository repository,
    IIntegrationEventIdempotencyGuard idempotencyGuard
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
                if (feedback is not null)
                {
                    repository.Delete(feedback);
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
