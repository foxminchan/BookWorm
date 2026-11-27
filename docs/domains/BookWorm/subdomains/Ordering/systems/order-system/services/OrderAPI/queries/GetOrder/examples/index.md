<Columns cols={2}>

<Column>

## 1. Retrieve Order Details and Line Items

An authenticated customer or admin views a specific order's status, total, timestamp, and line items (`GET /api/v1/orders/{id}`).

### Payload details

- `id`: Order UUID.
- `date`: UTC ISO 8601 order timestamp.
- `total`: Order total (`number`).
- `status`: One of `"New"`, `"Cancelled"`, or `"Completed"`.
- `items`: Array of order line items (`id`, `quantity`, `price`, and `name`).

### Example response

```json
{
	"id": "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99",
	"date": "2026-11-25T16:14:57Z",
	"total": 59.98,
	"status": "New",
	"items": [
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
			"quantity": 1,
			"price": 14.99,
			"name": "The Left Hand of Darkness"
		},
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12",
			"quantity": 2,
			"price": 22.5,
			"name": "Dune"
		}
	]
}
```

### Using this example

Returns `200 OK` with the `OrderDetailDto` JSON payload, or `404 Not Found` if the order does not exist or is not accessible to the caller.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Get("/api/v1/orders/{id}")]
    Task<OrderDetailResponse> GetOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}

public sealed record OrderItemResponse(Guid Id, int Quantity, decimal Price, string? Name);

public sealed record OrderDetailResponse(
    Guid Id,
    DateTime Date,
    decimal Total,
    string Status,
    IReadOnlyList<OrderItemResponse> Items
);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var order = await orderApiClient.GetOrderAsync(
    Guid.Parse("01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99"),
    cancellationToken
);
```

```typescript api-client.ts
import ordersApiClient from "@workspace/api-client/ordering/orders";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

const orderId = "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99";

// 1. Direct @workspace/api-client (Axios) call
const order = await ordersApiClient.get(orderId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: orderDetail, isLoading } = useOrder(orderId);
```

</CodeGroup>

</Column>

</Columns>
