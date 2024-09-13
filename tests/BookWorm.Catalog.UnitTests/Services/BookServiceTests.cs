using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.Grpc;
using BookWorm.Catalog.UnitTests.Builder;
using BookWorm.Catalog.UnitTests.Helpers;
using MediatR;
using Book = BookWorm.Catalog.Domain.BookAggregate.Book;

namespace BookWorm.Catalog.UnitTests.Services;
        
public sealed class BookServiceTests
{
    private readonly BookService _bookService;
    private readonly Mock<ISender> _senderMock;

    public BookServiceTests()
    {
        _senderMock = new();
        Mock<ILogger<BookService>> loggerMock = new();
        _bookService = new(_senderMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBook_ShouldReturnBookResponse_WhenBookExists()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[0];
        var bookId = book.Id;

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBookQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        // Act
        var response = await _bookService.GetBook(request, context);

        // Assert
        response.Should().NotBeNull();
        response.Book.Should().NotBeNull();
        response.Book.Id.Should().Be(book.Id.ToString());
        response.Book.Name.Should().Be(book.Name);
        response.Book.Price.Should().Be((double)book.Price!.OriginalPrice);
        response.Book.PriceSale.Should().Be((double)(book.Price.DiscountPrice ?? -1m));
    }

    [Fact]
    public async Task GetBook_ShouldReturnEmpty_WhenBookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid().ToString();

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBookQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null!);

        var request = new BookRequest { BookId = bookId };
        var context = new TestServerCallContext();

        // Act
        var response = await _bookService.GetBook(request, context);

        // Assert
        response
            .Should()
            .NotBeNull()
            .And
            .BeOfType<BookResponse>()
            .Which.Book.Should().BeNull();
    }

    [Fact]
    public async Task GetBookStatus_ShouldReturnBookStatusResponse_WhenBookExists()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[0];
        var bookId = book.Id;

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBookQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new BookStatusRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        // Act
        var response = await _bookService.GetBookStatus(request, context);

        // Assert
        response.Should().NotBeNull();
        response.BookStatus.Should().NotBeNull();
        response.BookStatus.Id.Should().Be(book.Id.ToString());
        response.BookStatus.Status.Should().Be(book.Status.ToString());
    }

    [Fact]
    public async Task GetBookStatus_ShouldReturnEmpty_WhenBookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid().ToString();

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBookQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null!);

        var request = new BookStatusRequest { BookId = bookId };
        var context = new TestServerCallContext();

        // Act
        var response = await _bookService.GetBookStatus(request, context);

        // Assert
        response
            .Should()
            .NotBeNull()
            .And
            .BeOfType<BookStatusResponse>()
            .Which.BookStatus.Should().BeNull();
    }
}
