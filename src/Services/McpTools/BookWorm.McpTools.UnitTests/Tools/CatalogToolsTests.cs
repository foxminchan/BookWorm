using BookWorm.McpTools.Grpc.Services.Book;
using BookWorm.McpTools.Models;
using BookWorm.McpTools.Tools;
using Grpc.Core;

namespace BookWorm.McpTools.UnitTests.Tools;

public sealed class CatalogToolsTests
{
    private readonly Mock<IBookService> _bookService = new();
    private readonly CatalogTools _sut;

    public CatalogToolsTests()
    {
        _sut = new(_bookService.Object);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task GivenEmptyDescription_WhenSearchCatalog_ThenShouldReturnValidationMessage(
        string description
    )
    {
        // Act
        var result = await _sut.SearchCatalogAsync(description);

        // Assert
        result.ShouldBe("Please provide a non-empty description to search the catalog.");
        _bookService.Verify(
            x => x.ListBooksAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenServiceThrowsRpcException_WhenSearchCatalog_ThenShouldReturnErrorMessage()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListBooksAsync("test", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new(StatusCode.Internal, "Internal error")));

        // Act
        var result = await _sut.SearchCatalogAsync("test");

        // Assert
        result.ShouldBe("There was an error while searching the catalog. Please try again later.");
    }

    [Test]
    public async Task GivenServiceReturnsEmptyList_WhenSearchCatalog_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListBooksAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.SearchCatalogAsync("nonexistent");

        // Assert
        result.ShouldBe(
            "We couldn't find any books matching your description. Please try again with a different description."
        );
    }

    [Test]
    public async Task GivenServiceReturnsBooks_WhenSearchCatalog_ThenShouldReturnSerializedJson()
    {
        // Arrange
        IReadOnlyList<Book> books =
        [
            new(
                BookId.From(Guid.CreateVersion7()),
                "Clean Code",
                "A book about writing clean code",
                "https://example.com/image.jpg",
                29.99m,
                null,
                new(CategoryId.From(Guid.CreateVersion7()), "Programming"),
                new(PublisherId.From(Guid.CreateVersion7()), "Prentice Hall"),
                [new(AuthorId.From(Guid.CreateVersion7()), "Robert C. Martin")],
                4.5,
                120
            ),
        ];

        _bookService
            .Setup(x => x.ListBooksAsync("clean code", It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _sut.SearchCatalogAsync("clean code");

        // Assert
        result.ShouldContain("Clean Code");
        result.ShouldContain("Robert C. Martin");
        result.ShouldContain("29.99");
    }

    [Test]
    public async Task GivenEmptyId_WhenGetBook_ThenShouldReturnValidationMessage()
    {
        // Act
        var result = await _sut.GetBookAsync(Guid.Empty);

        // Assert
        result.ShouldBe("Please provide a valid non-empty book ID.");
        _bookService.Verify(
            x => x.GetBookByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenServiceThrowsRpcException_WhenGetBook_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();

        _bookService
            .Setup(x => x.GetBookByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new(StatusCode.Internal, "Internal error")));

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldBe("There was an error retrieving the book. Please try again later.");
    }

    [Test]
    public async Task GivenServiceThrowsNotFoundRpcException_WhenGetBook_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();

        _bookService
            .Setup(x => x.GetBookByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new(StatusCode.NotFound, "Not found")));

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldBe($"No book found with ID {id}.");
    }

    [Test]
    public async Task GivenServiceReturnsNull_WhenGetBook_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();

        _bookService
            .Setup(x => x.GetBookByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldBe($"No book found with ID {id}.");
    }

    [Test]
    public async Task GivenServiceReturnsBook_WhenGetBook_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var book = new Book(
            BookId.From(id),
            "Clean Code",
            "A book about writing clean code",
            "https://example.com/image.jpg",
            29.99m,
            null,
            new(CategoryId.From(Guid.CreateVersion7()), "Programming"),
            new(PublisherId.From(Guid.CreateVersion7()), "Prentice Hall"),
            [new(AuthorId.From(Guid.CreateVersion7()), "Robert C. Martin")],
            4.5,
            120
        );

        _bookService
            .Setup(x => x.GetBookByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldContain("Clean Code");
        result.ShouldContain("29.99");
    }

    [Test]
    public async Task GivenServiceThrowsRpcException_WhenListCategories_ThenShouldReturnErrorMessage()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListCategoriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new(StatusCode.Internal, "Internal error")));

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldBe("There was an error retrieving categories. Please try again later.");
    }

    [Test]
    public async Task GivenServiceReturnsEmptyList_WhenListCategories_ThenShouldReturnNoCategories()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldBe("No categories are currently available.");
    }

    [Test]
    public async Task GivenServiceReturnsCategories_WhenListCategories_ThenShouldReturnSerializedJson()
    {
        // Arrange
        IReadOnlyList<Category> categories =
        [
            new(CategoryId.From(Guid.CreateVersion7()), "Programming"),
            new(CategoryId.From(Guid.CreateVersion7()), "Fiction"),
        ];

        _bookService
            .Setup(x => x.ListCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldContain("Programming");
        result.ShouldContain("Fiction");
    }

    [Test]
    public async Task GivenServiceThrowsRpcException_WhenListAuthors_ThenShouldReturnErrorMessage()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListAuthorsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new(StatusCode.Internal, "Internal error")));

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldBe("There was an error retrieving authors. Please try again later.");
    }

    [Test]
    public async Task GivenServiceReturnsEmptyList_WhenListAuthors_ThenShouldReturnNoAuthors()
    {
        // Arrange
        _bookService
            .Setup(x => x.ListAuthorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldBe("No authors are currently available.");
    }

    [Test]
    public async Task GivenServiceReturnsAuthors_WhenListAuthors_ThenShouldReturnSerializedJson()
    {
        // Arrange
        IReadOnlyList<Author> authors =
        [
            new(AuthorId.From(Guid.CreateVersion7()), "Robert C. Martin"),
            new(AuthorId.From(Guid.CreateVersion7()), "Martin Fowler"),
        ];

        _bookService
            .Setup(x => x.ListAuthorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authors);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldContain("Robert C. Martin");
        result.ShouldContain("Martin Fowler");
    }
}
