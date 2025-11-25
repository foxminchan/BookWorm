using BookWorm.Catalog.Grpc.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Ordering.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(BookGrpcService.BookGrpcServiceClient service, HybridCache cache)
    : IBookService
{
    public async Task<GetBookResponse?> GetBookByIdAsync(
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

    public async Task<GetBooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    )
    {
        var sortedIds = ids.OrderBy(x => x).ToArray();

        var result = await cache.GetOrCreateAsync(
            $"books:{string.Join(",", sortedIds)}",
            async _ =>
            {
                var response = await service.GetBooksAsync(
                    new() { BookIds = { sortedIds } },
                    cancellationToken: cancellationToken
                );
                return response;
            },
            tags: ["books", nameof(Catalog).ToLowerInvariant()],
            cancellationToken: cancellationToken
        );

        return result;
    }
}
