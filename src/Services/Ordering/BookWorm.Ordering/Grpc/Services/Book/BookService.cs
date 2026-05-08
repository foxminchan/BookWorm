using BookWorm.Catalog.Grpc.Services;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(BookGrpcService.BookGrpcServiceClient service, IFusionCache cache)
    : IBookService
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
        var sortedIds = ids.OrderBy(x => x).ToArray();

        var result = await cache.GetOrSetAsync(
            $"books:{string.Join(",", sortedIds)}",
            async ct =>
            {
                var response = await service.GetBooksAsync(
                    new() { BookIds = { sortedIds } },
                    deadline: DateTime.UtcNow.AddSeconds(10),
                    cancellationToken: ct
                );
                return response;
            },
            tags: ["books", nameof(Catalog).ToLowerInvariant()],
            token: cancellationToken
        );

        return result;
    }
}
