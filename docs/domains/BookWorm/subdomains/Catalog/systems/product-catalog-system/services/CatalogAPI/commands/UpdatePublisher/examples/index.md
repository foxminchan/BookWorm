<Columns cols={2}>

<Column>

## 1. Rename an Existing Publisher

A backoffice catalog manager updates a publisher's display name (`PUT /api/v1/publishers`).

### Payload details

- `id`: Required publisher UUID.
- `name`: Required non-empty updated publisher name.

### Example payload

```json
{
	"id": "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
	"name": "Ace Science Fiction"
}
```

### Using this example

Returns `200 OK` with the updated `Publisher` JSON payload and invalidates cached publisher queries.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Put("/api/v1/publishers")]
    Task<PublisherResponse> UpdatePublisherAsync(
        [Body] UpdatePublisherRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record UpdatePublisherRequest(Guid Id, string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var updated = await catalogApiClient.UpdatePublisherAsync(
    new(Guid.Parse("01960a1b-2000-7000-8a9b-0c1d2e3f4a02"), "Ace Science Fiction"),
    cancellationToken
);
```

```typescript api-client.ts
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import useUpdatePublisher from "@workspace/api-hooks/catalog/publishers/useUpdatePublisher";

const request = {
	id: "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
	name: "Ace Science Fiction",
};

// 1. Direct @workspace/api-client (Axios) call
const updatedPublisher = await publishersApiClient.update(request);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updatePublisher, isPending } = useUpdatePublisher({
	onSuccess: (publisher) => {
		console.log(`Updated publisher ${publisher.id}: ${publisher.name}`);
	},
});

updatePublisher(request);
```

</CodeGroup>

</Column>

</Columns>
