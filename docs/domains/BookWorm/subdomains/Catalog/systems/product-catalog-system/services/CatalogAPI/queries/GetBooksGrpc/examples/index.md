<Columns cols={2}>

<Column>

## 1. Batch-Enrich Basket or Order Line Items

**Basket API** and **Ordering API** call `book.v1.BookGrpcService/GetBooks` with a deduplicated list of book UUIDs to resolve titles, prices, and stock status in a single RPC.

### Payload details

- Request (`GetBooksRequest`): `book_ids` (`repeated string` of Catalog book UUIDs).
- Response (`GetBooksResponse`): `books` (`repeated GetBookResponse`).

### Using this example

Returns `StatusCode.OK` with `GetBooksResponse.Books` containing one `GetBookResponse` for each found book ID.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Catalog.Grpc.Services;
using Grpc.Net.ClientFactory;

// 1. Named gRPC client registration in Ordering API (src/Services/Ordering/BookWorm.Ordering/Grpc/Extensions.cs)
builder.Services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
    HttpUtilities
        .AsUrlBuilder()
        .WithScheme(builder.GetScheme())
        .WithHost(Constants.Aspire.Services.Catalog)
        .Build(),
    HealthStatus.Degraded,
    BookService.BooksClientName
);

// 2. Typed gRPC client invocation via GrpcClientFactory
var distinctIds = ids.Distinct(StringComparer.Ordinal).ToArray();

var response = await clientFactory
    .CreateClient<BookGrpcService.BookGrpcServiceClient>(BookService.BooksClientName)
    .GetBooksAsync(
        new() { BookIds = { distinctIds } },
        deadline: DateTime.UtcNow.AddSeconds(10),
        cancellationToken: cancellationToken
    );
```

</CodeGroup>

</Column>

</Columns>
