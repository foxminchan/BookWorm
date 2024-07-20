using BookWorm.Catalog.Grpc;
using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;

namespace BookWorm.Ordering.Grpc;

public sealed class BookService(Book.BookClient bookClient)
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
