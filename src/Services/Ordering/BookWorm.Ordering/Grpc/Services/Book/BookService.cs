using BookWorm.Catalog.Grpc.Services;
using Grpc.Net.ClientFactory;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(IFusionCache cache, GrpcClientFactory clientFactory)
    : IBookService
{
    internal const string BooksClientName = "catalog-books";
    internal const string DashboardClientName = "dashboard-catalog";

    public async Task<GetDashboardResponse> GetDashboardAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await clientFactory
            .CreateClient<BookGrpcService.BookGrpcServiceClient>(DashboardClientName)
            .GetDashboardAsync(
                new(),
                deadline: DateTime.UtcNow.AddSeconds(10),
                cancellationToken: cancellationToken
            );
    }

    public async Task<GetBookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await clientFactory
            .CreateClient<BookGrpcService.BookGrpcServiceClient>(BooksClientName)
            .GetBookAsync(
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
                var response = await clientFactory
                    .CreateClient<BookGrpcService.BookGrpcServiceClient>(BooksClientName)
                    .GetBooksAsync(
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
