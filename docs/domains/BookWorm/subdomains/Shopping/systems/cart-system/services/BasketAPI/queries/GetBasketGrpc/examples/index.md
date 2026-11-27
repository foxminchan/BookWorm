<Columns cols={2}>

<Column>

## 1. Retrieve Customer Basket During Order Checkout

During `CreateOrder`, **Ordering API** invokes `basket.v1.BasketGrpcService/GetBasket` on **Basket API** using an On-Behalf-Of (OBO) Entra ID user token to read the authenticated customer's basket items.

### Payload details

- Request (`google.protobuf.Empty`): No fields; **Basket API** resolves the shopper identity from the propagated bearer token.
- Response (`GetBasketResponse`):
  - `id`: Basket identifier string (empty string if no basket exists).
  - `items`: Repeated `Item` messages containing `id` (book UUID) and `quantity` (`int32`).

### Using this example

Returns `StatusCode.OK` with `GetBasketResponse` populated from Redis (`id` and `items`), or an empty `GetBasketResponse` (`id = ""`, `items = []`) when no basket exists for the customer.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Basket.Grpc.Services;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;

// 1. DI registration in Ordering API (src/Services/Ordering/BookWorm.Ordering/Grpc/Extensions.cs)
builder.Services
    .AddGrpcServiceReference<BasketGrpcService.BasketGrpcServiceClient>(
        HttpUtilities
            .AsUrlBuilder()
            .WithScheme(builder.GetScheme())
            .WithHost(Constants.Aspire.Services.Basket)
            .Build(),
        HealthStatus.Degraded
    )
    .AddUserTokenExchange(Constants.Aspire.Services.Basket);

// 2. Typed gRPC client invocation (src/Services/Ordering/BookWorm.Ordering/Grpc/Services/Basket/BasketService.cs)
internal sealed class BasketService(BasketGrpcService.BasketGrpcServiceClient service)
    : IBasketService
{
    public async Task<GetBasketResponse> GetBasket(CancellationToken cancellationToken = default)
    {
        var result = await service.GetBasketAsync(
            new(),
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return result;
    }
}
```

</CodeGroup>

</Column>

</Columns>
