<Columns cols={2}>

<Column>

## 1. Register a New Author in the Catalog

A backoffice catalog manager creates an author record for _"Ursula K. Le Guin"_ before linking her to book listings.

### Payload details

- `name`: Required non-empty display name of the author.

### Example payload

```json
{
	"name": "Ursula K. Le Guin"
}
```

### Using this example

Returns `201 Created` with the created `Author` JSON payload and invalidates the catalog authors list cache.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Post("/api/v1/authors")]
    Task<AuthorResponse> CreateAuthorAsync(
        [Body] CreateAuthorRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record CreateAuthorRequest(string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var author = await catalogApiClient.CreateAuthorAsync(
    new("Ursula K. Le Guin"),
    cancellationToken
);
```

```typescript api-client.ts
import authorsApiClient from "@workspace/api-client/catalog/authors";
import useCreateAuthor from "@workspace/api-hooks/catalog/authors/useCreateAuthor";

// 1. Direct @workspace/api-client (Axios) call
const author = await authorsApiClient.create({
	name: "Ursula K. Le Guin",
});

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createAuthor, isPending } = useCreateAuthor({
	onSuccess: (created) => {
		console.log(`Created author ${created.id}: ${created.name}`);
	},
});

createAuthor({ name: "Ursula K. Le Guin" });
```

</CodeGroup>

</Column>

</Columns>
