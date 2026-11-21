using BookWorm.Catalog.Grpc.Services;
using Grpc.Net.ClientFactory;

namespace BookWorm.Ordering.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
internal sealed class BookService(GrpcClientFactory clientFactory) : IBookService
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
        var distinctIds = ids.Distinct(StringComparer.Ordinal).ToArray();

        if (distinctIds.Length == 0)
        {
            return new();
        }

        return await clientFactory
            .CreateClient<BookGrpcService.BookGrpcServiceClient>(BooksClientName)
            .GetBooksAsync(
                new() { BookIds = { distinctIds } },
                deadline: DateTime.UtcNow.AddSeconds(10),
                cancellationToken: cancellationToken
            );
    }
}
