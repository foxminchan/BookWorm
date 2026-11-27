<Columns cols={2}>

<Column>

## 1. Delete a Publisher by UUID

A backoffice catalog manager removes a publisher record (`DELETE /api/v1/publishers/{id}`).

### Payload details

- `id`: Required publisher UUID passed as the route parameter.
- Request body: None.

### Using this example

Returns `204 No Content` and removes the publisher record from **Catalog API**.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Delete("/api/v1/publishers/{id}")]
    Task DeletePublisherAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await catalogApiClient.DeletePublisherAsync(
    Guid.Parse("01960a1b-2000-7000-8a9b-0c1d2e3f4a02"),
    cancellationToken
);
```

```typescript api-client.ts
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import useDeletePublisher from "@workspace/api-hooks/catalog/publishers/useDeletePublisher";

const publisherId = "01960a1b-2000-7000-8a9b-0c1d2e3f4a02";

// 1. Direct @workspace/api-client (Axios) call
await publishersApiClient.delete(publisherId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deletePublisher, isPending } = useDeletePublisher({
	onSuccess: () => {
		console.log(`Deleted publisher ${publisherId}`);
	},
});

deletePublisher(publisherId);
```

</CodeGroup>

</Column>

</Columns>
