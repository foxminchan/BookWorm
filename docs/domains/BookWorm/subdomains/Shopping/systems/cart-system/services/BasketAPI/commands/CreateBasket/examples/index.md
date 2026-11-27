<Columns cols={2}>

<Column>

## 1. Initialize a Shopping Basket with Multiple Books

An authenticated shopper adds two books from the catalog to start a new Redis-backed shopping basket via **Basket API**.

### Payload details

- `items`: Required non-empty array of basket line items.
- `items[].id`: Catalog book UUID string.
- `items[].quantity`: Positive `int32` quantity for each book.

### Example payload

```json
{
	"items": [
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10",
			"quantity": 1
		},
		{
			"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a11",
			"quantity": 2
		}
	]
}
```

### Using this example

Returns `201 Created` with the enriched `CustomerBasket` JSON payload (including `id` and catalog-enriched `name`, `price`, and `priceSale` per item) and persists the basket in Redis.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IBasketApiClient
{
    [Post("/api/v1/baskets")]
    Task<CustomerBasketResponse> CreateBasketAsync(
        [Body] CreateBasketRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record BasketItemRequest(string Id, int Quantity);
public sealed record CreateBasketRequest(IReadOnlyList<BasketItemRequest> Items);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IBasketApiClient>(settings, Services.Basket)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Basket}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var basket = await basketApiClient.CreateBasketAsync(
    new([
        new("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", 1),
        new("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a11", 2),
    ]),
    cancellationToken
);
```

```typescript api-client.ts
import basketApiClient from "@workspace/api-client/basket/baskets";
import useCreateBasket from "@workspace/api-hooks/basket/useCreateBasket";

// 1. Direct @workspace/api-client (Axios) call
const basket = await basketApiClient.create({
	items: [
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", quantity: 1 },
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a11", quantity: 2 },
	],
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createBasket, isPending } = useCreateBasket({
	onSuccess: (created) => {
		console.log(`Created basket ${created.id} with ${created.items.length} item(s)`);
	},
});

createBasket({
	items: [
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10", quantity: 1 },
		{ id: "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a11", quantity: 2 },
	],
});
```

</CodeGroup>

</Column>

</Columns>
