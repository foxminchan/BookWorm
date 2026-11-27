<Columns cols={2}>

<Column>

## 1. Retrieve All Catalog Authors

The storefront filter sidebar or backoffice book form loads all registered authors (`GET /api/v1/authors`).

### Payload details

- Response body: JSON array of `{ id, name }` author objects.

### Example response

```json
[
	{
		"id": "01960a1b-3000-7000-8a9b-0c1d2e3f4a03",
		"name": "Ursula K. Le Guin"
	},
	{
		"id": "01960a1b-3000-7000-8a9b-0c1d2e3f4a04",
		"name": "Frank Herbert"
	}
]
```

### Using this example

Returns `200 OK` with the JSON array of `Author` records.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Get("/api/v1/authors")]
    Task<IReadOnlyList<AuthorResponse>> ListAuthorsAsync(
        CancellationToken cancellationToken = default
    );
}

public sealed record AuthorResponse(Guid Id, string? Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var authors = await catalogApiClient.ListAuthorsAsync(cancellationToken);
```

```typescript api-client.ts
import authorsApiClient from "@workspace/api-client/catalog/authors";
import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";

// 1. Direct @workspace/api-client (Axios) call
const authors = await authorsApiClient.list();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: authorList, isLoading } = useAuthors();
```

</CodeGroup>

</Column>

</Columns>
