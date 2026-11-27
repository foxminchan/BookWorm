<Columns cols={2}>

<Column>

## 1. List All Catalog Categories for MCP Tools

**McpTools** calls `book.v1.BookGrpcService/ListCategories` on **Catalog API** to expose available book categories to AI assistant tools.

### Payload details

- Request (`ListCategoriesRequest`): Empty message.
- Response (`ListCategoriesResponse`): `categories` (`repeated CategoryItem` with `id` and `name`).

### Using this example

Returns `StatusCode.OK` with `ListCategoriesResponse.Categories` containing all catalog categories.

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
var response = await client.ListCategoriesAsync(
    new(),
    deadline: DateTime.UtcNow.AddSeconds(10),
    cancellationToken: cancellationToken
);
```

</CodeGroup>

</Column>

</Columns>
