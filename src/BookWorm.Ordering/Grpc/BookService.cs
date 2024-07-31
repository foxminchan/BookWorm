using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;
using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;

namespace BookWorm.Ordering.Grpc;

public sealed class BookService(GrpcBookClient bookClient)
{
    public async Task<BookItem> GetBook(Guid bookId)
    {
        var response = await bookClient.GetBookAsync(new() { BookId = bookId.ToString() });

        return MapBookItem(response.Book);
    }

    private static BookItem MapBookItem(GrpcBookItem book)
    {
        return new(Guid.Parse(book.Id), book.Name);
    }
}

public sealed record BookItem(Guid Id, string Name);
