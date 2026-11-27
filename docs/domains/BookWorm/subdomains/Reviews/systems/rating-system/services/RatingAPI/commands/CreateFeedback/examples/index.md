<Columns cols={2}>

<Column>

## 1. Submit a Book Review with Rating and Comment

An authenticated reader submits a 5-star review for _"The Left Hand of Darkness"_ on the storefront book detail page.

### Payload details

- `bookId`: Required UUID of the Catalog book being reviewed.
- `firstName`: Reviewer's first name (`string | null`).
- `lastName`: Reviewer's last name (`string | null`).
- `comment`: Optional written review (`string | null`).
- `rating`: Integer rating from `0` through `5`.

### Example payload

```json
{
	"bookId": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	"firstName": "Minh",
	"lastName": "Nguyen",
	"comment": "A landmark of anthropological science fiction with unforgettable world-building.",
	"rating": 5
}
```

### Using this example

Returns `201 Created` with the persisted `Feedback` JSON object and dispatches `FeedbackCreatedIntegrationEvent` to update the book's rating summary in **Catalog API**.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IRatingApiClient
{
    [Post("/api/v1/feedbacks")]
    Task<FeedbackResponse> CreateFeedbackAsync(
        [Body] CreateFeedbackRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record CreateFeedbackRequest(
    Guid BookId,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IRatingApiClient>(settings, Services.Rating)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Rating}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var feedback = await ratingApiClient.CreateFeedbackAsync(
    new(
        Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"),
        "Minh",
        "Nguyen",
        "A landmark of anthropological science fiction with unforgettable world-building.",
        5
    ),
    cancellationToken
);
```

```typescript api-client.ts
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import useCreateFeedback from "@workspace/api-hooks/rating/useCreateFeedback";

// 1. Direct @workspace/api-client (Axios) call
const feedback = await ratingApiClient.create({
	bookId: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	firstName: "Minh",
	lastName: "Nguyen",
	comment: "A landmark of anthropological science fiction with unforgettable world-building.",
	rating: 5,
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createFeedback, isPending } = useCreateFeedback({
	onSuccess: (created) => {
		console.log(`Submitted feedback ${created.id} with rating ${created.rating}`);
	},
});

createFeedback({
	bookId: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	firstName: "Minh",
	lastName: "Nguyen",
	comment: "A landmark of anthropological science fiction with unforgettable world-building.",
	rating: 5,
});
```

</CodeGroup>

</Column>

</Columns>
