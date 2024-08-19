namespace BookWorm.Ordering.Grpc;

public interface IBookService
{
    Task<BookItem> GetBookAsync(Guid bookId, CancellationToken cancellationToken = default);
}
