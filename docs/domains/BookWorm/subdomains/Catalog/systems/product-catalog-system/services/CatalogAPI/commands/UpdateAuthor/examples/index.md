<Columns cols={2}>

<Column>

## 1. Rename an Existing Author

A backoffice catalog manager updates an author's display name (`PUT /api/v1/authors`).

### Payload details

- `id`: Required author UUID.
- `name`: Required non-empty updated display name.

### Example payload

```json
{
	"id": "01960a1b-3000-7000-8a9b-0c1d2e3f4a03",
	"name": "Ursula Kroeber Le Guin"
}
```

### Using this example

Returns `200 OK` with the updated `Author` JSON payload and invalidates cached author queries.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Put("/api/v1/authors")]
    Task<AuthorResponse> UpdateAuthorAsync(
        [Body] UpdateAuthorRequest request,
        CancellationToken cancellationToken = default
    );
}

public sealed record UpdateAuthorRequest(Guid Id, string Name);

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
var updated = await catalogApiClient.UpdateAuthorAsync(
    new(Guid.Parse("01960a1b-3000-7000-8a9b-0c1d2e3f4a03"), "Ursula Kroeber Le Guin"),
    cancellationToken
);
```

```typescript api-client.ts
import authorsApiClient from "@workspace/api-client/catalog/authors";
import useUpdateAuthor from "@workspace/api-hooks/catalog/authors/useUpdateAuthor";

const request = {
	id: "01960a1b-3000-7000-8a9b-0c1d2e3f4a03",
	name: "Ursula Kroeber Le Guin",
};

// 1. Direct @workspace/api-client (Axios) call
const updatedAuthor = await authorsApiClient.update(request);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: updateAuthor, isPending } = useUpdateAuthor({
	onSuccess: (author) => {
		console.log(`Updated author ${author.id}: ${author.name}`);
	},
});

updateAuthor(request);
```

</CodeGroup>

</Column>

</Columns>
