<Columns cols={2}>

<Column>

## 1. Update Line-Item Quantities in an Existing Basket

An authenticated shopper adjusts the quantity of _"The Left Hand of Darkness"_ and adds _"Dune"_ to their existing basket.

### Payload details

- `items`: Required array representing the full replacement set of basket items.
- `items[].id`: Catalog book UUID string.
- `items[].quantity`: Updated `int32` quantity for each item.

### Example payload

```json
{
	"items": [
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
			"quantity": 3
		},
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12",
			"quantity": 1
		}
	]
}
```

### Using this example

Returns `200 OK` with the updated, catalog-enriched `CustomerBasket` JSON payload and replaces the stored items in Redis.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IBasketApiClient
{
    [Put("/api/v1/baskets")]
    Task<CustomerBasketResponse> UpdateBasketAsync(
        [Body] UpdateBasketRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record BasketItemRequest(string Id, int Quantity);
public sealed record UpdateBasketRequest(IReadOnlyList<BasketItemRequest> Items);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IBasketApiClient>(settings, Services.Basket)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Basket}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var updated = await basketApiClient.UpdateBasketAsync(
    new([
        new("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", 3),
        new("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12", 1),
    ]),
    cancellationToken
);
```

```typescript api-client.ts
import basketApiClient from "@workspace/api-client/basket/baskets";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";

// 1. Direct @workspace/api-client (Axios) call
const updatedBasket = await basketApiClient.update({
	items: [
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", quantity: 3 },
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12", quantity: 1 },
	],
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updateBasket, isPending } = useUpdateBasket({
	onSuccess: (basket) => {
		console.log(`Updated basket ${basket.id}`);
	},
});

updateBasket({
	items: [
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", quantity: 3 },
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a12", quantity: 1 },
	],
});
```

</CodeGroup>

</Column>

</Columns>
