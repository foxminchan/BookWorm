namespace BookWorm.McpTools.Grpc.Services;

public interface IBookService
{
    Task<BookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    );

    Task<BooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    );
}
