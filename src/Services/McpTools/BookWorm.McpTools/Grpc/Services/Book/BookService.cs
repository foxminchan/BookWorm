using BookWorm.Catalog.Grpc.Services;
using Grpc.Core;

namespace BookWorm.McpTools.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(BookGrpcService.BookGrpcServiceClient client) : IBookService
{
    public async Task<IReadOnlyList<Models.Book>> ListBooksAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await client.ListBooksAsync(
            new() { Search = search ?? string.Empty },
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return [.. response.Books.Select(MapToBook)];
    }

    public async Task<Models.Book?> GetBookByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await client.GetBookDetailsAsync(
                new() { BookId = id.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(10),
                cancellationToken: cancellationToken
            );

            return response.Book is null ? null : MapToBook(response.Book);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Category>> ListCategoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await client.ListCategoriesAsync(
            new(),
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return
        [
            .. response.Categories.Select(c => new Category(
                CategoryId.From(Guid.Parse(c.Id)),
                c.Name
            )),
        ];
    }

    public async Task<IReadOnlyList<Author>> ListAuthorsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await client.ListAuthorsAsync(
            new(),
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return
        [
            .. response.Authors.Select(a => new Author(AuthorId.From(Guid.Parse(a.Id)), a.Name)),
        ];
    }

    private static Models.Book MapToBook(BookItem item)
    {
        decimal? price = item.Price;
        decimal? priceSale = item.PriceSale;

        return new(
            BookId.From(Guid.Parse(item.Id)),
            item.Name,
            string.IsNullOrEmpty(item.Description) ? null : item.Description,
            string.IsNullOrEmpty(item.ImageUrl) ? null : item.ImageUrl,
            price ?? 0m,
            priceSale,
            item.Category is null
                ? null
                : new(CategoryId.From(Guid.Parse(item.Category.Id)), item.Category.Name),
            item.Publisher is null
                ? null
                : new(PublisherId.From(Guid.Parse(item.Publisher.Id)), item.Publisher.Name),
            [.. item.Authors.Select(a => new Author(AuthorId.From(Guid.Parse(a.Id)), a.Name))],
            item.AverageRating,
            item.TotalReviews
        );
    }
}
