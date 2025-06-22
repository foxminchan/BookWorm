using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Basket.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
public sealed class BookService(BookGrpcService.BookGrpcServiceClient service) : IBookService
{
    public async Task<BookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBookAsync(
            new() { BookId = id },
            cancellationToken: cancellationToken
        );

        return result;
    }

    public async Task<BooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBooksAsync(
            new() { BookIds = { ids } },
            cancellationToken: cancellationToken
        );

        return result;
    }
}
