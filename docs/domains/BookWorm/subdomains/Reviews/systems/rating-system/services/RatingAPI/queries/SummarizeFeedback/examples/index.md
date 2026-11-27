<Columns cols={2}>

<Column>

## 1. Summarize Reviews for a Top-Rated Book

A shopper or backoffice analyst requests an AI-generated sentiment summary for _"The Left Hand of Darkness"_ after multiple customer reviews have been recorded.

### Payload details

- `summary`: Generated narrative summary of customer feedback (required).
- `classification`: One of `"NoData"`, `"Bad"`, `"Good"`, or `"TopRated"`.
- `averageRating`: Arithmetic mean rating (`number | null`).
- `reviewCount`: Total reviews included (`int32`).
- `evidence`: Supporting excerpts or reasoning.
- `confidence`: Assessment confidence label (`""` when no feedback exists).

### Example response

```json
{
	"summary": "Readers consistently praise the novel's anthropological depth, prose style, and exploration of culture and identity.",
	"classification": "TopRated",
	"averageRating": 4.8,
	"reviewCount": 24,
	"evidence": "22 of 24 reviews rate the book 4 or 5 stars, highlighting world-building and character depth.",
	"confidence": "High"
}
```

### Using this example

Returns `200 OK` with a `SummarizeResult` payload containing the classification (`TopRated`), `averageRating`, and `reviewCount`.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IRatingApiClient
{
    [Get("/api/v1/feedbacks/{bookId}/summarize")]
    Task<SummarizeResultResponse> SummarizeFeedbackAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    );
}

public sealed record SummarizeResultResponse(
    string Summary,
    string? Classification,
    double? AverageRating,
    int? ReviewCount,
    string? Evidence,
    string? Confidence
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IRatingApiClient>(settings, Services.Rating)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Rating}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var summary = await ratingApiClient.SummarizeFeedbackAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"),
    cancellationToken
);
```

```typescript api-client.ts
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import useSummaryFeedback from "@workspace/api-hooks/rating/useSummaryFeedback";

const bookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10";

// 1. Direct @workspace/api-client (Axios) call
const summary = await ratingApiClient.summary(bookId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: feedbackSummary, isLoading } = useSummaryFeedback(bookId);
```

</CodeGroup>

</Column>

</Columns>

---

<Columns cols={2}>

<Column>

## 2. Book Without Existing Reviews (`NoData`)

When a newly published book has no submitted feedback yet, **Rating API** returns a deterministic `NoData` response without invoking the LLM.

### Payload details

- `classification`: `"NoData"` when no reviews exist.
- `averageRating`: `null` when `reviewCount` is `0`.
- `evidence` and `confidence`: Empty strings (`""`).

### Example response

```json
{
	"summary": "No customer feedback has been submitted for this book yet.",
	"classification": "NoData",
	"averageRating": null,
	"reviewCount": 0,
	"evidence": "",
	"confidence": ""
}
```

### Using this example

Returns `200 OK` with a deterministic `NoData` `SummarizeResult` payload without invoking the LLM.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
var summary = await ratingApiClient.SummarizeFeedbackAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a99"),
    cancellationToken
);
```

```typescript api-client.ts
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import useSummaryFeedback from "@workspace/api-hooks/rating/useSummaryFeedback";

const bookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a99";

// 1. Direct @workspace/api-client (Axios) call
const summary = await ratingApiClient.summary(bookId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: feedbackSummary, isLoading } = useSummaryFeedback(bookId);
```

</CodeGroup>

</Column>

</Columns>
