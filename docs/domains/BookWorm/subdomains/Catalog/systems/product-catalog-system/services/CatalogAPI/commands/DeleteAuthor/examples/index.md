<Columns cols={2}>

<Column>

## 1. Delete an Author by UUID

A backoffice catalog manager removes an unused author record (`DELETE /api/v1/authors/{id}`).

### Payload details

- `id`: Required author UUID passed as the route parameter.
- Request body: None.

### Using this example

Returns `204 No Content` and removes the author record from **Catalog API**.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Delete("/api/v1/authors/{id}")]
    Task DeleteAuthorAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await catalogApiClient.DeleteAuthorAsync(
    Guid.Parse("01960a1b-3000-7000-8a9b-0c1d2e3f4a03"),
    cancellationToken
);
```

```typescript api-client.ts
import authorsApiClient from "@workspace/api-client/catalog/authors";
import useDeleteAuthor from "@workspace/api-hooks/catalog/authors/useDeleteAuthor";

const authorId = "01960a1b-3000-7000-8a9b-0c1d2e3f4a03";

// 1. Direct @workspace/api-client (Axios) call
await authorsApiClient.delete(authorId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteAuthor, isPending } = useDeleteAuthor({
	onSuccess: () => {
		console.log(`Deleted author ${authorId}`);
	},
});

deleteAuthor(authorId);
```

</CodeGroup>

</Column>

</Columns>
