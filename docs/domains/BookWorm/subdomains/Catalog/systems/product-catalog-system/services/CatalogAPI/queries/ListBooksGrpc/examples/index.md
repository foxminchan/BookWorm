<Columns cols={2}>

<Column>

## 1. Search Books over gRPC for MCP Tools

**McpTools** calls `book.v1.BookGrpcService/ListBooks` on **Catalog API** with an optional search string to retrieve matching `BookItem` records.

### Payload details

- Request (`ListBooksRequest`): `search` (`string`, empty string returns unfiltered results).
- Response (`ListBooksResponse`): `books` (`repeated BookItem`).

### Using this example

Returns `StatusCode.OK` with `ListBooksResponse.Books` containing matching `BookItem` projections.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;

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
var response = await client.ListBooksAsync(
    new() { Search = "Le Guin" },
    deadline: DateTime.UtcNow.AddSeconds(10),
    cancellationToken: cancellationToken
);
```

</CodeGroup>

</Column>

</Columns>
