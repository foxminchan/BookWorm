<Columns cols={2}>

<Column>

## 1. Update Book Details with Optimistic Concurrency (`If-Match`)

A backoffice catalog manager updates the promotional price and description of _"The Left Hand of Darkness"_ while passing the `If-Match` ETag header for optimistic concurrency control.

### Payload details

- `id`: Required book UUID.
- `name`, `description`, `price`, `priceSale`, `categoryId`, `publisherId`, `authorIds`: Updated book fields sent as `multipart/form-data`.
- `isRemoveImage`: Optional boolean (`true` removes the existing cover image).

### Using this example

Returns `204 No Content` (or `412 Precondition Failed` if `If-Match` is stale), updates the book in PostgreSQL, invalidates FusionCache, and queues a vector embedding refresh.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Multipart]
    [Put("/api/v1/books")]
    Task UpdateBookAsync(
        [AliasAs("id")] Guid id,
        [AliasAs("name")] string name,
        [AliasAs("description")] string description,
        [AliasAs("price")] decimal price,
        [AliasAs("priceSale")] decimal? priceSale,
        [AliasAs("categoryId")] Guid categoryId,
        [AliasAs("publisherId")] Guid publisherId,
        [AliasAs("authorIds")] IEnumerable<Guid> authorIds,
        [AliasAs("isRemoveImage")] bool isRemoveImage = false,
        [Header("If-Match")] string? ifMatch = null,
        CancellationToken cancellationToken = default
    );
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await catalogApiClient.UpdateBookAsync(
    id: Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"),
    name: "The Left Hand of Darkness (50th Anniversary Edition)",
    description: "Anniversary edition with introduction by Neil Gaiman.",
    price: 21.99m,
    priceSale: 16.99m,
    categoryId: Guid.Parse("01960a1b-1000-7000-8a9b-0c1d2e3f4a01"),
    publisherId: Guid.Parse("01960a1b-2000-7000-8a9b-0c1d2e3f4a02"),
    authorIds: [Guid.Parse("01960a1b-3000-7000-8a9b-0c1d2e3f4a03")],
    isRemoveImage: false,
    ifMatch: "\"1\"",
    cancellationToken: cancellationToken
);
```

```typescript api-client.ts
import booksApiClient from "@workspace/api-client/catalog/books";
import useUpdateBook from "@workspace/api-hooks/catalog/books/useUpdateBook";

const request = {
	id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
	name: "The Left Hand of Darkness (50th Anniversary Edition)",
	description: "Anniversary edition with introduction by Neil Gaiman.",
	price: 21.99,
	priceSale: 16.99,
	categoryId: "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
	publisherId: "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
	authorIds: ["01960a1b-3000-7000-8a9b-0c1d2e3f4a03"],
	isRemoveImage: false,
};

// 1. Direct @workspace/api-client (Axios multipart/form-data) call
await booksApiClient.update(request, { ifMatch: '"1"' });

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updateBook, isPending } = useUpdateBook({
	onSuccess: () => {
		console.log("Book updated");
	},
});

updateBook({ request, ifMatch: '"1"' });
```

</CodeGroup>

</Column>

</Columns>
