<Columns cols={2}>

<Column>

## 1. List Paginated Orders Filtered by Status

An authenticated customer views their order history or a backoffice admin filters orders by status (`GET /api/v1/orders`).

### Payload details

- Query parameters: `pageIndex`, `pageSize`, `status` (`"New"`, `"Cancelled"`, or `"Completed"`).
- Response array items: `id` (UUID), `date` (ISO 8601 UTC), `total` (`number`), and `status`.

### Example response

```json
[
	{
		"id": "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99",
		"date": "2026-11-25T16:14:57Z",
		"total": 59.98,
		"status": "New"
	},
	{
		"id": "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a88",
		"date": "2026-11-20T09:30:00Z",
		"total": 22.5,
		"status": "Completed"
	}
]
```

### Using this example

Returns `200 OK` with a JSON array of `OrderDto` summaries and `Pagination-Count` / `Link` response headers.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Get("/api/v1/orders")]
    Task<IReadOnlyList<OrderSummaryResponse>> ListOrdersAsync(
        [Query] ListOrdersQuery query,
        CancellationToken cancellationToken = default
    );
}

public sealed record ListOrdersQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Status = null
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var orders = await orderApiClient.ListOrdersAsync(new(1, 20, "New"), cancellationToken);
```

```typescript api-client.ts
import ordersApiClient from "@workspace/api-client/ordering/orders";
import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";

const query = { pageIndex: 1, pageSize: 20, status: "New" as const };

// 1. Direct @workspace/api-client (Axios) call
const pagedOrders = await ordersApiClient.list(query);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: orders, isLoading } = useOrders(query);
```

</CodeGroup>

</Column>

</Columns>
