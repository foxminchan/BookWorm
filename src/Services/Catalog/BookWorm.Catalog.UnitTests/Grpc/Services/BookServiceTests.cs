using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Authors.List;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Categories.List;
using BookWorm.Catalog.Features.Dashboard;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Catalog.UnitTests.Grpc.Context;
using BookWorm.Chassis.Exceptions;
using BookWorm.SharedKernel.Results;
using Grpc.Core;
using Mediator;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
using Status = BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Status;

namespace BookWorm.Catalog.UnitTests.Grpc.Services;

public sealed class BookServiceTests : IDisposable
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookService _bookService;
    private readonly FusionCache _cache;
    private readonly Mock<ISender> _senderMock;

    public BookServiceTests()
    {
        _bookRepositoryMock = new();
        _senderMock = new();
        _cache = new(new FusionCacheOptions());
        var loggerMock = new Mock<ILogger<BookService>>();
        _bookService = new(
            _bookRepositoryMock.Object,
            loggerMock.Object,
            _senderMock.Object,
            _cache
        );
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GivenDashboardSummary_WhenGetDashboardCalled_ThenShouldReturnAllCategoryCounts()
    {
        var summary = new CatalogDashboardDto(35, [new("Fiction", 30), new("Other", 5)]);
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetCatalogDashboardQuery>(), CancellationToken.None)
            )
            .ReturnsAsync(summary);

        var result = await _bookService.GetDashboard(new(), new TestServerCallContext());

        result.TotalBooks.ShouldBe(35);
        result
            .Categories.Select(category => new CategoryCountDto(category.Name, category.Value))
            .ShouldBe(summary.Categories);
        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetCatalogDashboardQuery>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyCatalog_WhenGetDashboardCalled_ThenShouldReturnZeroBooksAndNoCategories()
    {
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetCatalogDashboardQuery>(), CancellationToken.None)
            )
            .ReturnsAsync(new CatalogDashboardDto(0, []));

        var result = await _bookService.GetDashboard(new(), new TestServerCallContext());

        result.TotalBooks.ShouldBe(0);
        result.Categories.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenCancellationToken_WhenGetDashboardCalled_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        _senderMock
            .Setup(sender => sender.Send(It.IsAny<GetCatalogDashboardQuery>(), cancellationToken))
            .ReturnsAsync(new CatalogDashboardDto(0, []));

        await _bookService.GetDashboard(
            new(),
            new TestServerCallContext(cancellationToken: cancellationToken)
        );

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetCatalogDashboardQuery>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDashboardQueryFailure_WhenGetDashboardCalled_ThenShouldPropagateException()
    {
        var exception = new InvalidOperationException("Catalog summary unavailable.");
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetCatalogDashboardQuery>(), CancellationToken.None)
            )
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(() =>
            _bookService.GetDashboard(new(), new TestServerCallContext())
        );

        result.ShouldBe(exception);
    }

    [Test]
    public async Task GivenValidBookId_WhenGetBookCalled_ThenShouldReturnBookResponse()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var bookRequest = new GetBookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new BookSummary(
            BookId.From(bookId),
            "Test Book",
            29.99m,
            19.99m,
            Status.InStock
        );

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None))
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
            repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentBookId_WhenGetBookCalled_ThenShouldThrowRpcException()
    {
        // Arrange
        var nonExistentBookId = Guid.CreateVersion7();
        var bookRequest = new GetBookRequest { BookId = nonExistentBookId.ToString() };
        var context = new TestServerCallContext();

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(nonExistentBookId, CancellationToken.None))
            .ReturnsAsync((BookSummary)null!);

        // Act & Assert
        var exception = await Should.ThrowAsync<RpcException>(async () =>
            await _bookService.GetBook(bookRequest, context)
        );

        exception.StatusCode.ShouldBe(StatusCode.NotFound);
        exception.Status.Detail.ShouldBe($"Book with id {nonExistentBookId} not found.");

        _bookRepositoryMock.Verify(
            repo => repo.GetSummaryByIdAsync(nonExistentBookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOutOfStockBook_WhenGetBookCalled_ThenShouldReturnOutOfStockStatus()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var bookRequest = new GetBookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var book = new BookSummary(
            BookId.From(bookId),
            "Out of Stock Book",
            29.99m,
            null,
            Status.OutOfStock
        );

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None))
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
        var booksRequest = new GetBooksRequest
        {
            BookIds = { bookIds.Select(id => id.ToString()) },
        };
        var context = new TestServerCallContext();

        List<BookSummary> books =
        [
            new(BookId.From(bookIds[0]), "Test Book 1", 29.99m, 19.99m, Status.InStock),
            new(BookId.From(bookIds[1]), "Test Book 2", 29.99m, 19.99m, Status.InStock),
        ];

        _bookRepositoryMock
            .Setup(repo =>
                repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None)
            )
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
            repo => repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDebugLoggingEnabled_WhenGetBookCalled_ThenShouldLogDebugMessage()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var bookRequest = new GetBookRequest { BookId = bookId.ToString() };
        var context = new TestServerCallContext();

        var loggerMock = new Mock<ILogger<BookService>>();

        // Set up the logger to return true for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

        var bookService = new BookService(
            _bookRepositoryMock.Object,
            loggerMock.Object,
            Mock.Of<ISender>(),
            _cache
        );

        var book = new BookSummary(
            BookId.From(bookId),
            "Test Book",
            29.99m,
            19.99m,
            Status.InStock
        );

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None))
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
                        (o, t) => o!.ToString()!.Contains($"Getting book status with id: {bookId}")
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
        var booksRequest = new GetBooksRequest
        {
            BookIds = { bookIds.Select(id => id.ToString()) },
        };
        var context = new TestServerCallContext();

        var loggerMock = new Mock<ILogger<BookService>>();

        // Set up the logger to return true for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

        var bookService = new BookService(
            _bookRepositoryMock.Object,
            loggerMock.Object,
            Mock.Of<ISender>(),
            _cache
        );

        List<BookSummary> books =
        [
            new(BookId.From(bookIds[0]), "Test Book 1", 29.99m, 19.99m, Status.InStock),
            new(BookId.From(bookIds[1]), "Test Book 2", 29.99m, 19.99m, Status.InStock),
        ];

        _bookRepositoryMock
            .Setup(repo =>
                repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None)
            )
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
                        (o, t) => o!.ToString()!.Contains("Getting book status with id")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSearchTerm_WhenListBooksCalled_ThenShouldReturnMappedBooks()
    {
        // Arrange
        var bookId = BookId.From(Guid.CreateVersion7());
        var categoryId = CategoryId.From(Guid.CreateVersion7());
        var publisherId = PublisherId.From(Guid.CreateVersion7());
        var authorId = AuthorId.From(Guid.CreateVersion7());
        IReadOnlyList<BookDto> dtos =
        [
            new(
                bookId,
                "Clean Code",
                "A handbook of agile software craftsmanship",
                "https://example.com/clean-code.jpg",
                29.99m,
                24.99m,
                Status.InStock,
                new(categoryId, "Programming"),
                new(publisherId, "Prentice Hall"),
                [new(authorId, "Robert C. Martin")],
                4.8,
                100
            ),
        ];
        PagedResult<BookDto> pagedResult = [with(dtos, 1, 10, 1)];

        _senderMock
            .Setup(s =>
                s.Send(It.Is<ListBooksQuery>(q => q.Search == "clean code"), CancellationToken.None)
            )
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.ListBooks(
            new() { Search = "clean code" },
            new TestServerCallContext()
        );

        // Assert
        result.Books.Count.ShouldBe(1);
        var item = result.Books[0];
        item.Id.ShouldBe(bookId.ToString());
        item.Name.ShouldBe("Clean Code");
        item.Description.ShouldBe("A handbook of agile software craftsmanship");
        item.ImageUrl.ShouldBe("https://example.com/clean-code.jpg");
        ((decimal?)item.Price).ShouldBe(29.99m);
        ((decimal?)item.PriceSale).ShouldBe(24.99m);
        item.Status.ShouldBe(BookStatus.InStock);
        item.Category.Name.ShouldBe("Programming");
        item.Publisher.Name.ShouldBe("Prentice Hall");
        item.Authors.Count.ShouldBe(1);
        item.Authors[0].Name.ShouldBe("Robert C. Martin");
        item.AverageRating.ShouldBe(4.8);
        item.TotalReviews.ShouldBe(100);
    }

    [Test]
    public async Task GivenValidBookId_WhenGetBookDetailsCalled_ThenShouldReturnBookDetails()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var dto = new BookDto(
            BookId.From(id),
            "Clean Code",
            "Description",
            "https://example.com/clean-code.jpg",
            29.99m,
            null,
            Status.OutOfStock,
            null,
            null,
            [],
            4.5,
            10
        );

        _senderMock
            .Setup(s => s.Send(new GetBookQuery(BookId.From(id)), CancellationToken.None))
            .ReturnsAsync(dto);

        // Act
        var result = await _bookService.GetBookDetails(
            new() { BookId = id.ToString() },
            new TestServerCallContext()
        );

        // Assert
        result.Book.ShouldNotBeNull();
        result.Book.Id.ShouldBe(id.ToString());
        result.Book.Name.ShouldBe("Clean Code");
        result.Book.Status.ShouldBe(BookStatus.OutOfStock);
        ((decimal?)result.Book.PriceSale).ShouldBeNull();
    }

    [Test]
    public async Task GivenInvalidBookId_WhenGetBookDetailsCalled_ThenShouldThrowInvalidArgumentRpcException()
    {
        // Act & Assert
        var ex = await Should.ThrowAsync<RpcException>(() =>
            _bookService.GetBookDetails(
                new() { BookId = "not-a-guid" },
                new TestServerCallContext()
            )
        );

        ex.StatusCode.ShouldBe(StatusCode.InvalidArgument);
    }

    [Test]
    public async Task GivenNonExistentBookId_WhenGetBookDetailsCalled_ThenShouldThrowNotFoundRpcException()
    {
        // Arrange
        var id = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(new GetBookQuery(BookId.From(id)), CancellationToken.None))
            .ThrowsAsync(NotFoundException.For<Book>(id));

        // Act & Assert
        var ex = await Should.ThrowAsync<RpcException>(() =>
            _bookService.GetBookDetails(
                new() { BookId = id.ToString() },
                new TestServerCallContext()
            )
        );

        ex.StatusCode.ShouldBe(StatusCode.NotFound);
    }

    [Test]
    public async Task GivenCategoriesExist_WhenListCategoriesCalled_ThenShouldReturnCategories()
    {
        // Arrange
        var categoryId = CategoryId.From(Guid.CreateVersion7());
        IReadOnlyList<CategoryDto> categories = [new(categoryId, "Programming")];

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCategoriesQuery>(), CancellationToken.None))
            .ReturnsAsync(categories);

        // Act
        var result = await _bookService.ListCategories(new(), new TestServerCallContext());

        // Assert
        result.Categories.Count.ShouldBe(1);
        result.Categories[0].Id.ShouldBe(categoryId.ToString());
        result.Categories[0].Name.ShouldBe("Programming");
    }

    [Test]
    public async Task GivenAuthorsExist_WhenListAuthorsCalled_ThenShouldReturnAuthors()
    {
        // Arrange
        var authorId = AuthorId.From(Guid.CreateVersion7());
        IReadOnlyList<AuthorDto> authors = [new(authorId, "Robert C. Martin")];

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListAuthorsQuery>(), CancellationToken.None))
            .ReturnsAsync(authors);

        // Act
        var result = await _bookService.ListAuthors(new(), new TestServerCallContext());

        // Assert
        result.Authors.Count.ShouldBe(1);
        result.Authors[0].Id.ShouldBe(authorId.ToString());
        result.Authors[0].Name.ShouldBe("Robert C. Martin");
    }

    [Test]
    public async Task GivenCachedBookSummary_WhenGetBookCalledTwice_ThenShouldReturnFromCache()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var request = new GetBookRequest { BookId = bookId.ToString() };
        var summary = new BookSummary(BookId.From(bookId), "Cached Book", 20m, 15m, Status.InStock);

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None))
            .ReturnsAsync(summary);

        // Act
        var first = await _bookService.GetBook(request, new TestServerCallContext());
        var second = await _bookService.GetBook(request, new TestServerCallContext());

        // Assert
        first.Name.ShouldBe("Cached Book");
        second.Name.ShouldBe("Cached Book");
        _bookRepositoryMock.Verify(
            repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMissingBook_WhenGetBookCalledTwice_ThenShouldNotCacheNotFound()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var request = new GetBookRequest { BookId = bookId.ToString() };

        _bookRepositoryMock
            .Setup(repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None))
            .ReturnsAsync((BookSummary)null!);

        // Act & Assert
        await Should.ThrowAsync<RpcException>(() =>
            _bookService.GetBook(request, new TestServerCallContext())
        );
        await Should.ThrowAsync<RpcException>(() =>
            _bookService.GetBook(request, new TestServerCallContext())
        );

        _bookRepositoryMock.Verify(
            repo => repo.GetSummaryByIdAsync(bookId, CancellationToken.None),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenPartialCacheHit_WhenGetBooksCalled_ThenShouldFetchOnlyMissingIdsAndPreserveOrder()
    {
        // Arrange
        var firstId = Guid.CreateVersion7();
        var secondId = Guid.CreateVersion7();
        var firstSummary = new BookSummary(
            BookId.From(firstId),
            "First Book",
            25m,
            null,
            Status.InStock
        );
        var secondSummary = new BookSummary(
            BookId.From(secondId),
            "Second Book",
            35m,
            30m,
            Status.InStock
        );

        await _cache.SetAsync(
            CatalogCacheKeys.BookSummary(firstId),
            firstSummary,
            tags: [CatalogCacheKeys.BookTag]
        );

        _bookRepositoryMock
            .Setup(repo =>
                repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None)
            )
            .ReturnsAsync([secondSummary]);

        // Act
        var firstResult = await _bookService.GetBooks(
            new() { BookIds = { firstId.ToString(), secondId.ToString(), firstId.ToString() } },
            new TestServerCallContext()
        );
        var secondResult = await _bookService.GetBooks(
            new() { BookIds = { secondId.ToString(), firstId.ToString() } },
            new TestServerCallContext()
        );

        // Assert
        firstResult.Books.Select(b => b.Id).ShouldBe([firstId.ToString(), secondId.ToString()]);
        secondResult.Books.Select(b => b.Id).ShouldBe([secondId.ToString(), firstId.ToString()]);
        _bookRepositoryMock.Verify(
            repo => repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBookSummary_WhenSerializedWithFusionCacheSerializer_ThenShouldRoundTripAllFields()
    {
        // Arrange
        var summary = new BookSummary(
            BookId.From(Guid.CreateVersion7()),
            "Refactoring",
            55.00m,
            44.00m,
            Status.InStock
        );
        var serializer = new FusionCacheSystemTextJsonSerializer();

        // Act
        var bytes = await serializer.SerializeAsync(summary);
        var deserialized = await serializer.DeserializeAsync<BookSummary>(bytes);

        // Assert
        deserialized.ShouldBe(summary);
    }

    [Test]
    public async Task GivenEmptyBookIds_WhenGetBooksCalled_ThenShouldReturnEmptyResponseDirectly()
    {
        // Arrange
        var request = new GetBooksRequest();

        // Act
        var result = await _bookService.GetBooks(request, new TestServerCallContext());

        // Assert
        result.ShouldNotBeNull();
        result.Books.ShouldBeEmpty();
        _bookRepositoryMock.Verify(
            repo =>
                repo.ListSummariesAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
