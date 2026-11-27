<Columns cols={2}>

<Column>

## 1. Rename an Existing Category

A backoffice catalog manager renames a category (`PUT /api/v1/categories`).

### Payload details

- `id`: Required category UUID.
- `name`: Required non-empty updated category name.

### Example payload

```json
{
	"id": "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
	"name": "Speculative & Science Fiction"
}
```

### Using this example

Returns `200 OK` with the updated `Category` JSON payload and invalidates cached category queries.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Put("/api/v1/categories")]
    Task<CategoryResponse> UpdateCategoryAsync(
        [Body] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record UpdateCategoryRequest(Guid Id, string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var updated = await catalogApiClient.UpdateCategoryAsync(
    new(Guid.Parse("01960a1b-1000-7000-8a9b-0c1d2e3f4a01"), "Speculative & Science Fiction"),
    cancellationToken
);
```

```typescript api-client.ts
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import useUpdateCategory from "@workspace/api-hooks/catalog/categories/useUpdateCategory";

const request = {
	id: "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
	name: "Speculative & Science Fiction",
};

// 1. Direct @workspace/api-client (Axios) call
const updatedCategory = await categoriesApiClient.update(request);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updateCategory, isPending } = useUpdateCategory({
	onSuccess: (category) => {
		console.log(`Updated category ${category.id}: ${category.name}`);
	},
});

updateCategory(request);
```

</CodeGroup>

</Column>

</Columns>
