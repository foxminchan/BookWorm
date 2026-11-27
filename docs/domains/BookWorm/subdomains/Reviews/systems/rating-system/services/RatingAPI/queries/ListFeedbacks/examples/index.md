<Columns cols={2}>

<Column>

## 1. Paginate Reviews for a Specific Book

The storefront book detail page queries **Rating API** for paginated reviews of _"The Left Hand of Darkness"_ ordered by rating descending.

### Payload details

- Query parameters: `bookId` (required book UUID), `pageIndex`, `pageSize`, `orderBy`, and `isDescending`.
- Response array items: `id`, `firstName`, `lastName`, `comment`, `rating` (`0`–`5`), and `bookId`.

### Example response

```json
[
	{
		"id": "01960a1b-5e6f-7a01-8a9b-0c1d2e3f4c01",
		"firstName": "Minh",
		"lastName": "Nguyen",
		"comment": "A landmark of anthropological science fiction with unforgettable world-building.",
		"rating": 5,
		"bookId": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"
	}
]
```

### Using this example

Returns `200 OK` with a JSON array of matching `Feedback` records and `Pagination-Count` / `Link` response headers.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IRatingApiClient
{
    [Get("/api/v1/feedbacks")]
    Task<IReadOnlyList<FeedbackResponse>> ListFeedbacksAsync(
        [Query] ListFeedbacksQuery query,
        CancellationToken cancellationToken = default
    );
}

public sealed record ListFeedbacksQuery(
    Guid BookId,
    int PageIndex = 1,
    int PageSize = 10,
    string? OrderBy = null,
    bool IsDescending = false
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IRatingApiClient>(settings, Services.Rating)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Rating}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var feedbacks = await ratingApiClient.ListFeedbacksAsync(
    new(Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"), 1, 10, "Rating", true),
    cancellationToken
);
```

```typescript api-client.ts
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";

const query = {
	bookId: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	pageIndex: 1,
	pageSize: 10,
	orderBy: "Rating",
	isDescending: true,
};

// 1. Direct @workspace/api-client (Axios) call
const pagedFeedbacks = await ratingApiClient.list(query);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: feedbacks, isLoading } = useFeedbacks(query);
```

</CodeGroup>

</Column>

</Columns>
