<Columns cols={2}>

<Column>

## 1. Register a New Publisher

A backoffice catalog manager registers _"Ace Books"_ as a publisher in **Catalog API**.

### Payload details

- `name`: Required non-empty publisher name.

### Example payload

```json
{
	"name": "Ace Books"
}
```

### Using this example

Returns `201 Created` with the created `Publisher` JSON payload and invalidates the publishers list cache.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Post("/api/v1/publishers")]
    Task<PublisherResponse> CreatePublisherAsync(
        [Body] CreatePublisherRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record CreatePublisherRequest(string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var publisher = await catalogApiClient.CreatePublisherAsync(
    new("Ace Books"),
    cancellationToken
);
```

```typescript api-client.ts
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import useCreatePublisher from "@workspace/api-hooks/catalog/publishers/useCreatePublisher";

// 1. Direct @workspace/api-client (Axios) call
const publisher = await publishersApiClient.create({
	name: "Ace Books",
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createPublisher, isPending } = useCreatePublisher({
	onSuccess: (created) => {
		console.log(`Created publisher ${created.id}: ${created.name}`);
	},
});

createPublisher({ name: "Ace Books" });
```

</CodeGroup>

</Column>

</Columns>
