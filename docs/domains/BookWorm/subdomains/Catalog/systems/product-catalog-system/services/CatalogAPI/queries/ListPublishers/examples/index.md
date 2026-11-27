<Columns cols={2}>

<Column>

## 1. Retrieve All Catalog Publishers

The storefront filter sidebar or backoffice book form loads all registered publishers (`GET /api/v1/publishers`).

### Payload details

- Response body: JSON array of `{ id, name }` publisher objects.

### Example response

```json
[
	{
		"id": "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
		"name": "Ace Books"
	},
	{
		"id": "01960a1b-2000-7000-8a9b-0c1d2e3f4a06",
		"name": "Gnome Press"
	}
]
```

### Using this example

Returns `200 OK` with the JSON array of `Publisher` records.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Get("/api/v1/publishers")]
    Task<IReadOnlyList<PublisherResponse>> ListPublishersAsync(
        CancellationToken cancellationToken = default
    );
}

public sealed record PublisherResponse(Guid Id, string? Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var publishers = await catalogApiClient.ListPublishersAsync(cancellationToken);
```

```typescript api-client.ts
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";

// 1. Direct @workspace/api-client (Axios) call
const publishers = await publishersApiClient.list();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: publisherList, isLoading } = usePublishers();
```

</CodeGroup>

</Column>

</Columns>
