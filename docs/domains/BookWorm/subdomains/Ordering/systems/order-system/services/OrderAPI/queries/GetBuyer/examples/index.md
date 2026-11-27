<Columns cols={2}>

<Column>

## 1. Retrieve a Buyer Profile by ID

An admin or backoffice user fetches a Buyer's profile and formatted shipping address by UUID (`GET /api/v1/buyers/{id}`).

### Payload details

- `id`: Buyer UUID.
- `name`: Buyer display name (`string | null`).
- `address`: Formatted shipping address (`string | null`).

### Example response

```json
{
	"id": "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01",
	"name": "Minh Nguyen",
	"address": "42 Nguyen Hue Blvd, Ho Chi Minh City, Ho Chi Minh"
}
```

### Using this example

Returns `200 OK` with the `BuyerDto` JSON payload, or `404 Not Found` if the Buyer does not exist.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IOrderApiClient
{
    [Get("/api/v1/buyers/{id}")]
    Task<BuyerResponse> GetBuyerAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record BuyerResponse(Guid Id, string? Name, string? Address);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<IOrderApiClient>(settings, Services.Ordering)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Ordering}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var buyer = await orderApiClient.GetBuyerAsync(
    Guid.Parse("01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01"),
    cancellationToken
);
```

```typescript api-client.ts
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";

const buyerId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a01";

// 1. Direct @workspace/api-client (Axios) call
const buyer = await buyersApiClient.get(buyerId);
const currentBuyer = await buyersApiClient.getCurrentBuyer();

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { data: buyerDetail, isLoading } = useBuyer(buyerId);
```

</CodeGroup>

</Column>

</Columns>
