<Columns cols={2}>

<Column>

## 1. Place an Order from the Authenticated Customer's Basket

An authenticated customer submits checkout; **Ordering API** fetches the shopper's basket via **Basket API** gRPC, enriches book prices via **Catalog API** gRPC, persists the `New` order, and emits `UserCheckedOutIntegrationEvent`.

### Payload details

- `x-request-id`: Required UUID idempotency key header (`HttpHeaders.RequestId`).
- Request body: Empty on the wire (`ordersApiClient.create(basketId)` passes `x-request-id` and invalidates the basket/order queries).

### Using this example

Returns `201 Created` with the generated `OrderId` UUID string, persists the order in PostgreSQL, and publishes `UserCheckedOutIntegrationEvent` to trigger **Finance**'s checkout orchestration.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Post("/api/v1/orders")]
    Task<Guid> CreateOrderAsync(
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
Guid orderId = await orderApiClient.CreateOrderAsync(
    Guid.CreateVersion7(),
    cancellationToken
);
```

```typescript api-client.ts
import ordersApiClient from "@workspace/api-client/ordering/orders";
import useCreateOrder from "@workspace/api-hooks/ordering/orders/useCreateOrder";

const basketId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01";

// 1. Direct @workspace/api-client (Axios) call (sets x-request-id automatically)
const orderId = await ordersApiClient.create(basketId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createOrder, isPending } = useCreateOrder({
	onSuccess: (createdOrderId) => {
		console.log(`Placed order ${createdOrderId}`);
	},
});

createOrder(basketId);
```

</CodeGroup>

</Column>

</Columns>
