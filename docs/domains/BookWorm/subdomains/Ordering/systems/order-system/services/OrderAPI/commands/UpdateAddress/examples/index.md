<Columns cols={2}>

<Column>

## 1. Update the Authenticated Buyer's Shipping Address

An authenticated customer updates their saved street, city, and province in **Ordering API** (`PATCH /api/v1/buyers/address`).

### Payload details

- `street`: Required street address line.
- `city`: Required city name.
- `province`: Required province or state name.

### Example payload

```json
{
	"street": "120 Le Loi Blvd",
	"city": "Ho Chi Minh City",
	"province": "Ho Chi Minh"
}
```

### Using this example

Returns `200 OK` with the updated `Buyer` JSON payload reflecting the new shipping address.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Patch("/api/v1/buyers/address")]
    Task<BuyerResponse> UpdateAddressAsync(
        [Body] UpdateAddressRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record UpdateAddressRequest(string Street, string City, string Province);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var buyer = await orderApiClient.UpdateAddressAsync(
    new("120 Le Loi Blvd", "Ho Chi Minh City", "Ho Chi Minh"),
    cancellationToken
);
```

```typescript api-client.ts
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";

// 1. Direct @workspace/api-client (Axios) call
const updatedBuyer = await buyersApiClient.updateAddress({
	street: "120 Le Loi Blvd",
	city: "Ho Chi Minh City",
	province: "Ho Chi Minh",
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updateAddress, isPending } = useUpdateBuyerAddress({
	onSuccess: (buyer) => {
		console.log(`Updated address for buyer ${buyer.id}`);
	},
});

updateAddress({
	street: "120 Le Loi Blvd",
	city: "Ho Chi Minh City",
	province: "Ho Chi Minh",
});
```

</CodeGroup>

</Column>

</Columns>
