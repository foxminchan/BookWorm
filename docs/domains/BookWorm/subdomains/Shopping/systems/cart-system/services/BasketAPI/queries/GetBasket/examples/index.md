<Columns cols={2}>

<Column>

## 1. Retrieve the Authenticated Shopper's Enriched Basket

An authenticated shopper opens the cart page; **Basket API** loads the items from Redis and enriches each book's `name`, `price`, and `priceSale` via **Catalog API** gRPC.

### Payload details

- `id`: Shopper/basket identifier (`string | null`).
- `items`: Array of enriched basket items (`id`, `quantity`, `name`, `price`, and nullable `priceSale`).

### Example response

```json
{
	"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01",
	"items": [
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
			"quantity": 1,
			"name": "The Left Hand of Darkness",
			"price": 18.99,
			"priceSale": 14.99
		},
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12",
			"quantity": 2,
			"name": "Dune",
			"price": 22.5,
			"priceSale": null
		}
	]
}
```

### Using this example

Returns `200 OK` with the `CustomerBasketDto` JSON payload containing current catalog titles and prices.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IBasketApiClient
{
    [Get("/api/v1/baskets")]
    Task<CustomerBasketResponse> GetBasketAsync(CancellationToken cancellationToken = default);
}

public sealed record BasketItemResponse(
    string? Id,
    int Quantity,
    string? Name,
    double Price,
    double? PriceSale
);

public sealed record CustomerBasketResponse(string? Id, IReadOnlyList<BasketItemResponse> Items);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IBasketApiClient>(settings, Services.Basket)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Basket}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var basket = await basketApiClient.GetBasketAsync(cancellationToken);
```

```typescript api-client.ts
import basketApiClient from "@workspace/api-client/basket/baskets";
import useBasket from "@workspace/api-hooks/basket/useBasket";

// 1. Direct @workspace/api-client (Axios) call
const basket = await basketApiClient.get();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: basketData, isLoading } = useBasket();
```

</CodeGroup>

</Column>

</Columns>
