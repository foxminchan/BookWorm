<Columns cols={2}>

<Column>

## 1. Retrieve All Catalog Categories

The storefront navigation menu or backoffice book form loads all available categories (`GET /api/v1/categories`).

### Payload details

- Response body: JSON array of `{ id, name }` category objects.

### Example response

```json
[
	{
		"id": "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
		"name": "Science Fiction"
	},
	{
		"id": "01960a1b-1000-7000-8a9b-0c1d2e3f4a05",
		"name": "Software Architecture"
	}
]
```

### Using this example

Returns `200 OK` with the JSON array of `Category` records.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Get("/api/v1/categories")]
    Task<IReadOnlyList<CategoryResponse>> ListCategoriesAsync(
        CancellationToken cancellationToken = default
    );
}

public sealed record CategoryResponse(Guid Id, string? Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var categories = await catalogApiClient.ListCategoriesAsync(cancellationToken);
```

```typescript api-client.ts
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

// 1. Direct @workspace/api-client (Axios) call
const categories = await categoriesApiClient.list();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: categoryList, isLoading } = useCategories();
```

</CodeGroup>

</Column>

</Columns>
