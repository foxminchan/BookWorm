using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Basket.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(BookGrpcService.BookGrpcServiceClient service) : IBookService
{
    public async Task<GetBookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBookAsync(
            new() { BookId = id },
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return result;
    }

    public async Task<GetBooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBooksAsync(
            new() { BookIds = { ids } },
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return result;
    }
}
