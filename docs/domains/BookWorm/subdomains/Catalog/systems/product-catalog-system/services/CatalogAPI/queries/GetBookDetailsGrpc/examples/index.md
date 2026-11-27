<Columns cols={2}>

<Column>

## 1. Retrieve Full Book Details for MCP Tools

**McpTools** calls `book.v1.BookGrpcService/GetBookDetails` on **Catalog API** to fetch a book's full metadata, category, publisher, authors, and rating summary.

### Payload details

- Request (`GetBookDetailsRequest`): `book_id` (Catalog book UUID string).
- Response (`GetBookDetailsResponse`): `book` (`BookItem` with `id`, `name`, `description`, `image_url`, `price`, `price_sale`, `status`, `category`, `publisher`, `authors`, `average_rating`, and `total_reviews`).

### Using this example

Returns `StatusCode.OK` with `GetBookDetailsResponse.Book` populated, or throws `RpcException` with `StatusCode.NotFound` if the book ID does not exist.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;
using Grpc.Core;

// 1. DI registration in McpTools (src/Services/McpTools/BookWorm.McpTools/Grpc/Extensions.cs)
builder.Services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
    HttpUtilities
        .AsUrlBuilder()
        .WithScheme(builder.GetScheme())
        .WithHost(Constants.Aspire.Services.Catalog)
        .Build(),
    HealthStatus.Degraded
);

// 2. Typed gRPC client invocation (src/Services/McpTools/BookWorm.McpTools/Grpc/Services/Book/BookService.cs)
var response = await client.GetBookDetailsAsync(
    new() { BookId = "01960a1b-2c3d-7e4f-8a9b-0c1d2e3f4a10" },
    deadline: DateTime.UtcNow.AddSeconds(10),
    cancellationToken: cancellationToken
);
```

</CodeGroup>

</Column>

</Columns>
