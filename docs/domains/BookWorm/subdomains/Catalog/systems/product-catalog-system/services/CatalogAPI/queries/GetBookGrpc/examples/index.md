<Columns cols={2}>

<Column>

## 1. Fetch Book Pricing and Availability During Basket or Order Validation

**Basket API** or **Ordering API** invokes `book.v1.BookGrpcService/GetBook` on **Catalog API** to verify a book's current price, sale price, and stock status.

### Payload details

- Request (`GetBookRequest`): `book_id` (Catalog book UUID string).
- Response (`GetBookResponse`): `id`, `name`, `price` (`DecimalValue`), `price_sale` (`DecimalValue`), and `status` (`BookStatus`).

### Using this example

Returns `StatusCode.OK` with `GetBookResponse` containing authoritative pricing and `BookStatus`, or `StatusCode.NotFound` when the book ID is missing.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;

// 1. DI registration in Basket API (src/Services/Basket/BookWorm.Basket/Grpc/Extensions.cs)
builder.Services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
    HttpUtilities
        .AsUrlBuilder()
        .WithScheme(builder.GetScheme())
        .WithHost(Constants.Aspire.Services.Catalog)
        .Build(),
    HealthStatus.Degraded
);

// 2. Typed gRPC client invocation (src/Services/Basket/BookWorm.Basket/Grpc/Services/Book/BookService.cs)
var result = await service.GetBookAsync(
    new() { BookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10" },
    deadline: DateTime.UtcNow.AddSeconds(10),
    cancellationToken: cancellationToken
);
```

</CodeGroup>

</Column>

</Columns>
