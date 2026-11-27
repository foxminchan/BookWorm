<Columns cols={2}>

<Column>

## 1. Remove an Existing Book Review

A moderator or admin deletes a specific feedback record by its UUID.

### Payload details

- `id`: Required feedback UUID passed as the route parameter (`/api/v1/feedbacks/{id}`).
- Request body: None (`additionalProperties: false` in `schema.json`).

### Using this example

Returns `204 No Content`, deletes the feedback record from **Rating API**, and publishes `FeedbackDeletedIntegrationEvent` so **Catalog API** recalculates the book's rating summary.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IRatingApiClient
{
    [Delete("/api/v1/feedbacks/{id}")]
    Task DeleteFeedbackAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IRatingApiClient>(settings, Services.Rating)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Rating}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await ratingApiClient.DeleteFeedbackAsync(
    Guid.Parse("01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01"),
    cancellationToken
);
```

```typescript api-client.ts
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import useDeleteFeedback from "@workspace/api-hooks/rating/useDeleteFeedback";

const feedbackId = "01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01";

// 1. Direct @workspace/api-client (Axios) call
await ratingApiClient.delete(feedbackId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteFeedback, isPending } = useDeleteFeedback({
	onSuccess: () => {
		console.log(`Deleted feedback ${feedbackId}`);
	},
});

deleteFeedback(feedbackId);
```

</CodeGroup>

</Column>

</Columns>
