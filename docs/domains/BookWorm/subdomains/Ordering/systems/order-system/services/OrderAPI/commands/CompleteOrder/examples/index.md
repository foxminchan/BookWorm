<Columns cols={2}>

<Column>

## 1. Mark an Order as Completed

An admin or fulfillment workflow completes a `New` order via **Ordering API** using an idempotent `x-request-id` header.

### Payload details

- `orderId`: UUID of the order passed in the route (`/api/v1/orders/{orderId}/complete`).
- `x-request-id`: Required UUID idempotency key header (`HttpHeaders.RequestId`).
- Request body: None.

### Using this example

Returns `200 OK` with the updated `Order` JSON payload (`status: "Completed"`) and dispatches `OrderStatusChangedToCompleteIntegrationEvent`.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Patch("/api/v1/orders/{orderId}/complete")]
    Task<OrderResponse> CompleteOrderAsync(
        Guid orderId,
        [Header("x-request-id")] Guid requestId,
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
var completed = await orderApiClient.CompleteOrderAsync(
    Guid.Parse("01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99"),
    Guid.CreateVersion7(),
    cancellationToken
);
```

```typescript api-client.ts
import ordersApiClient from "@workspace/api-client/ordering/orders";
import useCompleteOrder from "@workspace/api-hooks/ordering/orders/useCompleteOrder";

const orderId = "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99";

// 1. Direct @workspace/api-client (Axios) call (sets x-request-id automatically)
const completedOrder = await ordersApiClient.complete(orderId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: completeOrder, isPending } = useCompleteOrder({
	onSuccess: (order) => {
		console.log(`Order ${order.id} transitioned to ${order.status}`);
	},
});

completeOrder(orderId);
```

</CodeGroup>

</Column>

</Columns>
