<Columns cols={2}>

<Column>

## 1. Delete a Buyer Profile

An admin removes a Buyer record by its UUID via **Ordering API**.

### Payload details

- `id`: Required Buyer UUID passed as the route parameter (`/api/v1/buyers/{id}`).
- Request body: None.

### Using this example

Returns `204 No Content` and removes the Buyer record from PostgreSQL.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Delete("/api/v1/buyers/{id}")]
    Task DeleteBuyerAsync(Guid id, CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await orderApiClient.DeleteBuyerAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01"),
    cancellationToken
);
```

```typescript api-client.ts
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import useDeleteBuyer from "@workspace/api-hooks/ordering/buyers/useDeleteBuyer";

const buyerId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01";

// 1. Direct @workspace/api-client (Axios) call
await buyersApiClient.delete(buyerId);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteBuyer, isPending } = useDeleteBuyer({
	onSuccess: () => {
		console.log(`Deleted buyer ${buyerId}`);
	},
});

deleteBuyer(buyerId);
```

</CodeGroup>

</Column>

</Columns>
