using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Catalog.UnitTests.Grpc.Context;
using Microsoft.Extensions.Logging;

namespace BookWorm.Catalog.UnitTests.Grpc.Services;

public sealed class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new();
        var loggerMock = new Mock<ILogger<BookService>>();
        _bookService = new(_bookRepositoryMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task GivenValidBookId_WhenGetBookCalled_ThenShouldReturnBookResponse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var bookRequest = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new Book(
            "Test Book",
            "Test Description",
            "test-image.jpg",
            29.99m,
            19.99m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        // Use reflection to set the id property since it's not settable directly
        typeof(Book).GetProperty("Id")!.SetValue(book, bookId);

        _bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(bookId, CancellationToken.None))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBook(bookRequest, context);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(bookId.ToString());
        result.Name.ShouldBe("Test Book");
        result.Price.ShouldBe(29.99d);
        result.PriceSale.ShouldBe(19.99d);
        result.Status.ShouldBe(BookStatus.InStock);

        _bookRepositoryMock.Verify(
            repo => repo.GetByIdAsync(bookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentBookId_WhenGetBookCalled_ThenShouldReturnEmptyResponse()
    {
        // Arrange
        var nonExistentBookId = Guid.NewGuid();
        var bookRequest = new BookRequest { BookId = nonExistentBookId.ToString() };
        var context = new TestServerCallContext();

        _bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(nonExistentBookId, CancellationToken.None))
            .ReturnsAsync((Book)null!);

        // Act
        var result = await _bookService.GetBook(bookRequest, context);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEmpty();
        result.Name.ShouldBeEmpty();

        _bookRepositoryMock.Verify(
            repo => repo.GetByIdAsync(nonExistentBookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOutOfStockBook_WhenGetBookCalled_ThenShouldReturnOutOfStockStatus()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var bookRequest = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new Book(
            "Out of Stock Book",
            "Test Description",
            "test-image.jpg",
            29.99m,
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        // Use reflection to set the id and Status properties
        typeof(Book).GetProperty("Id")!.SetValue(book, bookId);
        typeof(Book).GetProperty("Status")!.SetValue(book, Status.OutOfStock);

        _bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(bookId, CancellationToken.None))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBook(bookRequest, context);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(bookId.ToString());
        result.Status.ShouldBe(BookStatus.OutOfStock);
    }
}
