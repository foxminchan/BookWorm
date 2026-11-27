<Columns cols={2}>

<Column>

## 1. Register a Buyer Shipping Profile

An authenticated customer creates their Buyer profile with a shipping address before placing their first order.

### Payload details

- `street`: Required street address line.
- `city`: Required city name.
- `province`: Required province or state name.

### Example payload

```json
{
	"street": "42 Nguyen Hue Blvd",
	"city": "Ho Chi Minh City",
	"province": "Ho Chi Minh"
}
```

### Using this example

Returns `201 Created` with the created `Buyer` JSON payload and `Location` header, persisted idempotently for the `x-request-id` key.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Post("/api/v1/buyers")]
    Task<BuyerResponse> CreateBuyerAsync(
        [Body] CreateBuyerRequest request,
        [Header("x-request-id")] Guid requestId,
        CancellationToken cancellationToken = default
    );
}

public sealed record CreateBuyerRequest(string Street, string City, string Province);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var buyer = await orderApiClient.CreateBuyerAsync(
    new("42 Nguyen Hue Blvd", "Ho Chi Minh City", "Ho Chi Minh"),
    Guid.CreateVersion7(),
    cancellationToken
);
```

```typescript api-client.ts
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import useCreateBuyer from "@workspace/api-hooks/ordering/buyers/useCreateBuyer";

// 1. Direct @workspace/api-client (Axios) call (sets x-request-id automatically)
const buyer = await buyersApiClient.create({
	street: "42 Nguyen Hue Blvd",
	city: "Ho Chi Minh City",
	province: "Ho Chi Minh",
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createBuyer, isPending } = useCreateBuyer({
	onSuccess: (created) => {
		console.log(`Created buyer ${created.id}`);
	},
});

createBuyer({
	street: "42 Nguyen Hue Blvd",
	city: "Ho Chi Minh City",
	province: "Ho Chi Minh",
});
```

</CodeGroup>

</Column>

</Columns>
