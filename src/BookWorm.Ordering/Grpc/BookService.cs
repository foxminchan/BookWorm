using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;
using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;

namespace BookWorm.Ordering.Grpc;

public sealed class BookService(GrpcBookClient bookClient) : IBookService
{
    public async Task<BookItem> GetBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var response = await bookClient.GetBookAsync(new() { BookId = bookId.ToString() },
            cancellationToken: cancellationToken);

        return MapBookItem(response.Book);
    }

    private static BookItem MapBookItem(GrpcBookItem book)
    {
        return new(Guid.Parse(book.Id), book.Name);
    }
}

public sealed record BookItem(Guid Id, string Name);
