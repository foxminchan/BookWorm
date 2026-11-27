<Columns cols={2}>

<Column>

## 1. Fetch Catalog Inventory Metrics for the Ordering Dashboard

**Ordering API** invokes `book.v1.BookGrpcService/GetDashboard` using a named gRPC client configured with OBO user token exchange to retrieve total book counts and category breakdowns.

### Payload details

- Request (`GetDashboardRequest`): Empty message.
- Response (`GetDashboardResponse`): `total_books` (`int64`) and `categories` (`repeated DashboardCategory` with `name` and `value`).

### Using this example

Returns `StatusCode.OK` with `GetDashboardResponse` (`total_books` and per-category counts) after validating the exchanged `business_user` Entra ID token.

</Column>

<Column>

<CodeGroup dropdown>

```csharp GrpcClient.cs
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;
using Grpc.Net.ClientFactory;

// 1. Named gRPC client + OBO token exchange in Ordering API (src/Services/Ordering/BookWorm.Ordering/Grpc/Extensions.cs)
builder.Services
    .AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
        HttpUtilities
            .AsUrlBuilder()
            .WithScheme(builder.GetScheme())
            .WithHost(Constants.Aspire.Services.Catalog)
            .Build(),
        HealthStatus.Degraded,
        BookService.DashboardClientName
    )
    .AddUserTokenExchange(Constants.Aspire.Services.Catalog);

// 2. Typed gRPC client invocation via GrpcClientFactory
var response = await clientFactory
    .CreateClient<BookGrpcService.BookGrpcServiceClient>(BookService.DashboardClientName)
    .GetDashboardAsync(
        new(),
        deadline: DateTime.UtcNow.AddSeconds(10),
        cancellationToken: cancellationToken
    );
```

</CodeGroup>

</Column>

</Columns>
