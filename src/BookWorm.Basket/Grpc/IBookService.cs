namespace BookWorm.Basket.Grpc;

public interface IBookService
{
    Task<BookStatus> GetBookStatusAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<bool> IsBookExistsAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<BookItem> GetBookAsync(Guid bookId, CancellationToken cancellationToken = default);
}
