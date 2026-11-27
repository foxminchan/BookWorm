<Columns cols={2}>

<Column>

## 1. Fetch Book Details for the Storefront Product Page

A storefront visitor opens the product page for _"The Left Hand of Darkness"_ (`GET /api/v1/books/{id}`), optionally passing `If-None-Match` for conditional ETag caching.

### Payload details

- `id`: Book UUID.
- `name`, `description`, `imageUrl`: Book metadata and cover image URI.
- `price`, `priceSale`: Regular decimal price and nullable promotional sale price.
- `status`: `"InStock"` or `"OutOfStock"`.
- `category`, `publisher`, `authors`: Embedded reference objects (`id`, `name`).
- `averageRating`, `totalReviews`: Aggregated rating metrics.

### Example response

```json
{
	"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	"name": "The Left Hand of Darkness",
	"description": "A science-fiction novel by Ursula K. Le Guin set on the icy planet Gethen.",
	"imageUrl": "https://cdn.bookworm.dev/catalog-blob/left-hand-of-darkness.jpg",
	"price": 18.99,
	"priceSale": 14.99,
	"status": "InStock",
	"category": {
		"id": "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
		"name": "Science Fiction"
	},
	"publisher": {
		"id": "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
		"name": "Ace Books"
	},
	"authors": [
		{
			"id": "01960a1b-3000-7000-8a9b-0c1d2e3f4a03",
			"name": "Ursula K. Le Guin"
		}
	],
	"averageRating": 4.8,
	"totalReviews": 24
}
```

### Using this example

Returns `200 OK` with the `GetBookResponse` JSON payload (or `304 Not Modified` when `If-None-Match` matches the current ETag).

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Get("/api/v1/books/{id}")]
    Task<BookResponse> GetBookAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record CatalogReferenceResponse(Guid Id, string? Name);

public sealed record BookResponse(
    Guid Id,
    string? Name,
    string? Description,
    Uri? ImageUrl,
    decimal Price,
    decimal? PriceSale,
    string Status,
    CatalogReferenceResponse? Category,
    CatalogReferenceResponse? Publisher,
    IReadOnlyList<CatalogReferenceResponse> Authors,
    double AverageRating,
    int TotalReviews
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var book = await catalogApiClient.GetBookAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"),
    cancellationToken
);
```

```typescript api-client.ts
import booksApiClient from "@workspace/api-client/catalog/books";
import useBook from "@workspace/api-hooks/catalog/books/useBook";

const bookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10";

// 1. Direct @workspace/api-client (Axios) call
const book = await booksApiClient.get(bookId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: bookDetail, isLoading } = useBook(bookId);
```

</CodeGroup>

</Column>

</Columns>
