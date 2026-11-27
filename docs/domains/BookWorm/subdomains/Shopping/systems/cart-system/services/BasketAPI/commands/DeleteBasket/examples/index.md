<Columns cols={2}>

<Column>

## 1. Clear the Authenticated Shopper's Basket

An authenticated shopper clears their shopping basket from the storefront cart drawer. The basket owner is resolved from the Entra ID JWT subject claim, so no request body or path ID is required.

### Payload details

- Request body: Empty (`additionalProperties: false` in `schema.json`).
- Identity: Extracted server-side from the authenticated `Authorization: Bearer <token>` header.

### Using this example

Returns `204 No Content` and deletes the shopper's basket key from Redis.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IBasketApiClient
{
    [Delete("/api/v1/baskets")]
    Task DeleteBasketAsync(CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IBasketApiClient>(settings, Services.Basket)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Basket}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await basketApiClient.DeleteBasketAsync(cancellationToken);
```

```typescript api-client.ts
import basketApiClient from "@workspace/api-client/basket/baskets";
import useDeleteBasket from "@workspace/api-hooks/basket/useDeleteBasket";

// 1. Direct @workspace/api-client (Axios) call
await basketApiClient.delete();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: deleteBasket, isPending } = useDeleteBasket({
	onSuccess: () => {
		console.log("Shopping basket cleared");
	},
});

deleteBasket();
```

</CodeGroup>

</Column>

</Columns>
