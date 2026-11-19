namespace BookWorm.McpTools.Grpc.Services.Book;

public interface IBookService
{
    Task<IReadOnlyList<Models.Book>> ListBooksAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<Models.Book?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListCategoriesAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Author>> ListAuthorsAsync(CancellationToken cancellationToken = default);
}
