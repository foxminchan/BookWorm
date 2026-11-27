<Columns cols={2}>

<Column>

## 1. Search and Filter Books with HTTP `QUERY`

A storefront shopper filters the catalog for science-fiction titles between \$10 and \$30 using the HTTP `QUERY` method on `/api/v1/books`.

### Payload details

- Request body (`ListBooksQuery`): `pageIndex`, `pageSize`, `orderBy`, `isDescending`, `search`, `minPrice`, `maxPrice`, `categoryId`, `publisherId`, `authorIds`.
- Response body: JSON array of book summaries matching `ListBooksResponse`, accompanied by `Pagination-Count` and `Link` headers.

### Example response

```json
[
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
]
```

### Using this example

Returns `200 OK` with a JSON array of matching `Book` records and `Pagination-Count` / `Link` headers.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryMethodAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => new("QUERY");
}

public interface ICatalogApiClient
{
    [QueryMethod("/api/v1/books")]
    Task<IReadOnlyList<BookResponse>> ListBooksAsync(
        [Body] ListBooksRequest query,
        CancellationToken cancellationToken = default
    );
}

public sealed record ListBooksRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? OrderBy = null,
    bool IsDescending = false,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid[]? CategoryId = null,
    Guid[]? AuthorIds = null
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var books = await catalogApiClient.ListBooksAsync(
    new(Search: "left hand", MinPrice: 10m, MaxPrice: 30m),
    cancellationToken
);
```

```typescript api-client.ts
import booksApiClient from "@workspace/api-client/catalog/books";
import useBooks from "@workspace/api-hooks/catalog/books/useBooks";

const query = {
	pageIndex: 1,
	pageSize: 20,
	orderBy: "Name",
	isDescending: false,
	search: "left hand",
	minPrice: 10,
	maxPrice: 30,
	categoryId: ["01960a1b-1000-7000-8a9b-0c1d2e3f4a01"],
	authorIds: ["01960a1b-3000-7000-8a9b-0c1d2e3f4a03"],
};

// 1. Direct @workspace/api-client (Axios HTTP QUERY) call
const pagedBooks = await booksApiClient.list(query);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: books, isLoading } = useBooks(query);
```

</CodeGroup>

</Column>

</Columns>
