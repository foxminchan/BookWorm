<Columns cols={2}>

<Column>

## 1. Create a Book Listing with Promotional Price and Cover Image

A backoffice catalog manager publishes _"The Left Hand of Darkness"_ with a regular price, promotional sale price, category, publisher, author, and a JPEG cover image via `multipart/form-data`.

### Payload details

- `name`: Required book title (max 50 chars).
- `description`: Required book summary.
- `price`: Required regular decimal price (`> 0`).
- `priceSale`: Optional promotional price (`> 0` and `<= price`).
- `categoryId`, `publisherId`: Required existing UUIDs.
- `authorIds`: Required non-empty array of author UUIDs.
- `image`: Optional cover image (`image/jpeg`, `image/png`, or `image/webp`, max 1 MB).

### Using this example

Returns `201 Created` with the generated UUIDv7 `BookId` and `Location: /api/v1/books/{id}` header, invalidates FusionCache book tags, and queues the book for vector indexing.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface ICatalogApiClient
{
    [Multipart]
    [Post("/api/v1/books")]
    Task<Guid> CreateBookAsync(
        [AliasAs("name")] string name,
        [AliasAs("description")] string description,
        [AliasAs("price")] decimal price,
        [AliasAs("priceSale")] decimal? priceSale,
        [AliasAs("categoryId")] Guid categoryId,
        [AliasAs("publisherId")] Guid publisherId,
        [AliasAs("authorIds")] IEnumerable<Guid> authorIds,
        [AliasAs("image")] StreamPart? image = null,
        CancellationToken cancellationToken = default
    );
}

// DI registration following BookWorm.Chassis Refit pattern
builder.Services
    .AddRefitGeneratedClient<ICatalogApiClient>(settings, Services.Catalog)
    .ConfigureHttpClient(client => client.BaseAddress = new($"https+http://{Services.Catalog}"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
await using var coverStream = File.OpenRead("cover.jpg");
var bookId = await catalogApiClient.CreateBookAsync(
    name: "The Left Hand of Darkness",
    description: "A science-fiction novel by Ursula K. Le Guin set on the icy planet Gethen.",
    price: 18.99m,
    priceSale: 14.99m,
    categoryId: Guid.Parse("01960a1b-1000-7000-8a9b-0c1d2e3f4a01"),
    publisherId: Guid.Parse("01960a1b-2000-7000-8a9b-0c1d2e3f4a02"),
    authorIds: [Guid.Parse("01960a1b-3000-7000-8a9b-0c1d2e3f4a03")],
    image: new StreamPart(coverStream, "cover.jpg", "image/jpeg"),
    cancellationToken: cancellationToken
);
```

```typescript api-client.ts
import booksApiClient from "@workspace/api-client/catalog/books";
import useCreateBook from "@workspace/api-hooks/catalog/books/useCreateBook";

const request = {
	name: "The Left Hand of Darkness",
	description: "A science-fiction novel by Ursula K. Le Guin set on the icy planet Gethen.",
	price: 18.99,
	priceSale: 14.99,
	categoryId: "01960a1b-1000-7000-8a9b-0c1d2e3f4a01",
	publisherId: "01960a1b-2000-7000-8a9b-0c1d2e3f4a02",
	authorIds: ["01960a1b-3000-7000-8a9b-0c1d2e3f4a03"],
	image: coverFile,
};

// 1. Direct @workspace/api-client (Axios multipart/form-data) call
const createdBook = await booksApiClient.create(request);

// 2. React / TanStack Query hook (@workspace/api-hooks)
const { mutate: createBook, isPending } = useCreateBook({
	onSuccess: () => {
		console.log("Book listing created");
	},
});

createBook(request);
```

</CodeGroup>

</Column>

</Columns>
