<Columns cols={2}>

<Column>

## 1. Create a New Book Category

A backoffice catalog manager adds a _"Science Fiction"_ category to organize catalog listings.

### Payload details

- `name`: Required non-empty category name.

### Example payload

```json
{
	"name": "Science Fiction"
}
```

### Using this example

Returns `201 Created` with the created `Category` JSON payload and invalidates the categories list cache.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Post("/api/v1/categories")]
    Task<CategoryResponse> CreateCategoryAsync(
        [Body] CreateCategoryRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record CreateCategoryRequest(string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var category = await catalogApiClient.CreateCategoryAsync(
    new("Science Fiction"),
    cancellationToken
);
```

```typescript api-client.ts
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import useCreateCategory from "@workspace/api-hooks/catalog/categories/useCreateCategory";

// 1. Direct @workspace/api-client (Axios) call
const category = await categoriesApiClient.create({
	name: "Science Fiction",
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createCategory, isPending } = useCreateCategory({
	onSuccess: (created) => {
		console.log(`Created category ${created.id}: ${created.name}`);
	},
});

createCategory({ name: "Science Fiction" });
```

</CodeGroup>

</Column>

</Columns>
