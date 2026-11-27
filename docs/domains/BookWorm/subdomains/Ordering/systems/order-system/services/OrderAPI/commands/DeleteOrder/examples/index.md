<Columns cols={2}>

<Column>

## 1. Soft-Delete an Order

An admin soft-deletes an order record by its UUID via **Ordering API**.

### Payload details

- `id`: Required Order UUID passed as the route parameter (`/api/v1/orders/{id}`).
- Request body: None.

### Using this example

Returns `204 No Content` and marks the order as soft-deleted in PostgreSQL.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Delete("/api/v1/orders/{id}")]
    Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await orderApiClient.DeleteOrderAsync(
    Guid.Parse("01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99"),
    cancellationToken
);
```

```typescript api-client.ts
import ordersApiClient from "@workspace/api-client/ordering/orders";
import useDeleteOrder from "@workspace/api-hooks/ordering/orders/useDeleteOrder";

const orderId = "01960a1b-3c4d-7e00-8a9b-0c1d2e3f4a99";

// 1. Direct @workspace/api-client (Axios) call
await ordersApiClient.delete(orderId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteOrder, isPending } = useDeleteOrder({
	onSuccess: () => {
		console.log(`Soft-deleted order ${orderId}`);
	},
});

deleteOrder(orderId);
```

</CodeGroup>

</Column>

</Columns>
