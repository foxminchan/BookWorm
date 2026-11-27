<Columns cols={2}>

<Column>

## 1. Paginate Registered Buyers in Backoffice

An admin opens the **Backoffice** buyers table to browse paginated buyer profiles (`GET /api/v1/buyers`).

### Payload details

- Query parameters: `pageIndex`, `pageSize`.
- Response array items: `id` (UUID), `name` (`string | null`), and `address` (`string | null`).

### Example response

```json
[
	{
		"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01",
		"name": "Minh Nguyen",
		"address": "42 Nguyen Hue Blvd, Ho Chi Minh City, Ho Chi Minh"
	}
]
```

### Using this example

Returns `200 OK` with a JSON array of `BuyerDto` records and `Pagination-Count` / `Link` response headers.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Get("/api/v1/buyers")]
    Task<IReadOnlyList<BuyerResponse>> ListBuyersAsync(
        [Query] int pageIndex = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default
    );
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var buyers = await orderApiClient.ListBuyersAsync(1, 20, cancellationToken);
```

```typescript api-client.ts
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import useBuyers from "@workspace/api-hooks/ordering/buyers/useBuyers";

const query = { pageIndex: 1, pageSize: 20 };

// 1. Direct @workspace/api-client (Axios) call
const pagedBuyers = await buyersApiClient.list(query);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: buyers, isLoading } = useBuyers(query);
```

</CodeGroup>

</Column>

</Columns>
