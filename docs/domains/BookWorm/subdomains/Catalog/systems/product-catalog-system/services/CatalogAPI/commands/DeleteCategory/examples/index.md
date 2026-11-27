<Columns cols={2}>

<Column>

## 1. Delete a Category by UUID

A backoffice catalog manager removes a category record (`DELETE /api/v1/categories/{id}`).

### Payload details

- `id`: Required category UUID passed as the route parameter.
- Request body: None.

### Using this example

Returns `204 No Content` and removes the category record from **Catalog API**.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Delete("/api/v1/categories/{id}")]
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await catalogApiClient.DeleteCategoryAsync(
    Guid.Parse("01960a1b-1000-7000-8a9b-0c1d2e3f4a01"),
    cancellationToken
);
```

```typescript api-client.ts
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import useDeleteCategory from "@workspace/api-hooks/catalog/categories/useDeleteCategory";

const categoryId = "01960a1b-1000-7000-8a9b-0c1d2e3f4a01";

// 1. Direct @workspace/api-client (Axios) call
await categoriesApiClient.delete(categoryId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteCategory, isPending } = useDeleteCategory({
	onSuccess: () => {
		console.log(`Deleted category ${categoryId}`);
	},
});

deleteCategory(categoryId);
```

</CodeGroup>

</Column>

</Columns>
