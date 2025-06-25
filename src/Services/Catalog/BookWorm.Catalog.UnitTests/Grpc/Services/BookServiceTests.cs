using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Catalog.UnitTests.Grpc.Context;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Status = BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Status;

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
        var bookId = Guid.CreateVersion7();
        var bookRequest = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new Book(
            "Test Book",
            "Test Description",
            "test-image.jpg",
            29.99m,
            19.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
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
        result.Price.ShouldBe(29.99m);
        result.PriceSale.ShouldBe(19.99m);
        result.Status.ShouldBe(BookStatus.InStock);

        _bookRepositoryMock.Verify(
            repo => repo.GetByIdAsync(bookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentBookId_WhenGetBookCalled_ThenShouldThrowRpcException()
    {
        // Arrange
        var nonExistentBookId = Guid.CreateVersion7();
        var bookRequest = new BookRequest { BookId = nonExistentBookId.ToString() };
        var context = new TestServerCallContext();

        _bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(nonExistentBookId, CancellationToken.None))
            .ReturnsAsync((Book)null!);

        // Act & Assert
        var exception = await Should.ThrowAsync<RpcException>(async () =>
            await _bookService.GetBook(bookRequest, context)
        );

        exception.StatusCode.ShouldBe(StatusCode.NotFound);
        exception.Status.Detail.ShouldBe($"Book with id {nonExistentBookId} not found.");

        _bookRepositoryMock.Verify(
            repo => repo.GetByIdAsync(nonExistentBookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOutOfStockBook_WhenGetBookCalled_ThenShouldReturnOutOfStockStatus()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var bookRequest = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new Book(
            "Out of Stock Book",
            "Test Description",
            "test-image.jpg",
            29.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
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

    [Test]
    public async Task GivenValidBookIds_WhenGetBooksCalled_ThenShouldReturnBooksResponse()
    {
        // Arrange
        var bookIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var booksRequest = new BooksRequest { BookIds = { bookIds.Select(id => id.ToString()) } };
        var context = new TestServerCallContext();

        List<Book> books =
        [
            new(
                "Test Book 1",
                "Test Description",
                "test-image.jpg",
                29.99m,
                19.99m,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            ),
            new(
                "Test Book 2",
                "Test Description",
                "test-image.jpg",
                29.99m,
                19.99m,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            ),
        ];
        // Use reflection to set the id properties since they're not settable directly
        typeof(Book).GetProperty("Id")!.SetValue(books[0], bookIds[0]);
        typeof(Book).GetProperty("Id")!.SetValue(books[1], bookIds[1]);

        _bookRepositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None))
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.GetBooks(booksRequest, context);

        // Assert
        result.ShouldNotBeNull();
        result.Books.Count.ShouldBe(2);
        result.Books[0].Id.ShouldBe(bookIds[0].ToString());
        result.Books[0].Name.ShouldBe("Test Book 1");
        result.Books[0].Price.ShouldBe(29.99m);
        result.Books[0].PriceSale.ShouldBe(19.99m);
        result.Books[0].Status.ShouldBe(BookStatus.InStock);

        result.Books[1].Id.ShouldBe(bookIds[1].ToString());
        result.Books[1].Name.ShouldBe("Test Book 2");
        result.Books[1].Price.ShouldBe(29.99m);
        result.Books[1].PriceSale.ShouldBe(19.99m);
        result.Books[1].Status.ShouldBe(BookStatus.InStock);

        _bookRepositoryMock.Verify(
            repo => repo.ListAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDebugLoggingEnabled_WhenGetBookCalled_ThenShouldLogDebugMessage()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var bookRequest = new BookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var loggerMock = new Mock<ILogger<BookService>>();

        // Set up the logger to return true for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

        var bookService = new BookService(_bookRepositoryMock.Object, loggerMock.Object);

        var book = new Book(
            "Test Book",
            "Test Description",
            "test-image.jpg",
            29.99m,
            19.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );
        typeof(Book).GetProperty("Id")!.SetValue(book, bookId);

        _bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(bookId, CancellationToken.None))
            .ReturnsAsync(book);

        // Act
        await bookService.GetBook(bookRequest, context);

        // Assert
        loggerMock.Verify(x => x.IsEnabled(LogLevel.Debug), Times.Once);

        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains($"Getting book status with id: {bookId}")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDebugLoggingEnabled_WhenGetBooksCalled_ThenShouldLogDebugMessage()
    {
        // Arrange
        var bookIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var booksRequest = new BooksRequest { BookIds = { bookIds.Select(id => id.ToString()) } };
        var context = new TestServerCallContext();

        var loggerMock = new Mock<ILogger<BookService>>();

        // Set up the logger to return true for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

        var bookService = new BookService(_bookRepositoryMock.Object, loggerMock.Object);

        List<Book> books =
        [
            new(
                "Test Book 1",
                "Test Description",
                "test-image.jpg",
                29.99m,
                19.99m,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            ),
            new(
                "Test Book 2",
                "Test Description",
                "test-image.jpg",
                29.99m,
                19.99m,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            ),
        ];
        typeof(Book).GetProperty("Id")!.SetValue(books[0], bookIds[0]);
        typeof(Book).GetProperty("Id")!.SetValue(books[1], bookIds[1]);

        _bookRepositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None))
            .ReturnsAsync(books);

        // Act
        await bookService.GetBooks(booksRequest, context);

        // Assert
        loggerMock.Verify(x => x.IsEnabled(LogLevel.Debug), Times.Once);

        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Getting book status with id")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
