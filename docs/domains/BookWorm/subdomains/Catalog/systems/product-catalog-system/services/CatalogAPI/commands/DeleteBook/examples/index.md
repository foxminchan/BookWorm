<Columns cols={2}>

<Column>

## 1. Soft-Delete a Book Listing

A backoffice catalog manager archives an out-of-print book listing (`DELETE /api/v1/books/{id}`).

### Payload details

- `id`: Required book UUID passed as the route parameter.
- Request body: None.

### Using this example

Returns `204 No Content`, soft-deletes the book in PostgreSQL, and invalidates its FusionCache entries.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Delete("/api/v1/books/{id}")]
    Task DeleteBookAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await catalogApiClient.DeleteBookAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10"),
    cancellationToken
);
```

```typescript api-client.ts
import booksApiClient from "@workspace/api-client/catalog/books";
import useDeleteBook from "@workspace/api-hooks/catalog/books/useDeleteBook";

const bookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10";

// 1. Direct @workspace/api-client (Axios) call
await booksApiClient.delete(bookId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteBook, isPending } = useDeleteBook({
	onSuccess: () => {
		console.log(`Deleted book ${bookId}`);
	},
});

deleteBook(bookId);
```

</CodeGroup>

</Column>

</Columns>
