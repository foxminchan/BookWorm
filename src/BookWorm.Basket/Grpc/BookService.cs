using BookWorm.Catalog.Grpc;
using GrpcBookStatus = BookWorm.Catalog.Grpc.BookStatus;
using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;

namespace BookWorm.Basket.Grpc;

public sealed class BookService(Book.BookClient bookClient)
{
    public async Task<BookStatus> GetBookStatus(Guid bookId)
    {
        var response = await bookClient.GetBookStatusAsync(new() { BookId = bookId.ToString() });

        return MapBookStatus(response.BookStatus);
    }

    public async Task<bool> IsBookExists(Guid bookId)
    {
        var response = await bookClient.GetBookAsync(new() { BookId = bookId.ToString() });

        return response.Book is not null;
    }

    public async Task<BookItem> GetBook(Guid bookId)
    {
        var response = await bookClient.GetBookAsync(new() { BookId = bookId.ToString() });

        return MapBookItem(response.Book);
    }

    private static BookStatus MapBookStatus(GrpcBookStatus status)
    {
        return new(Guid.Parse(status.Id), status.Status);
    }

    private static BookItem MapBookItem(GrpcBookItem book)
    {
        return new(Guid.Parse(book.Id), book.Name, (decimal)book.Price, (decimal)book.PriceSale);
    }
}

public sealed record BookStatus(Guid Id, string Status);

public sealed record BookItem(Guid Id, string Name, decimal Price, decimal PriceSale);
