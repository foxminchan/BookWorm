using System.Net;
using BookWorm.McpTools.Models;
using BookWorm.McpTools.Services;
using BookWorm.McpTools.Tools;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.UnitTests.Tools;

public sealed class ProductTests
{
    private readonly Mock<ICatalogApi> _catalogApi = new();
    private readonly Product _sut;

    public ProductTests()
    {
        _sut = new(_catalogApi.Object);
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenSearchCatalog_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var response = new ApiResponse<PagedResult<Book>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _catalogApi.Setup(x => x.ListBooksAsync(It.IsAny<string?>())).ReturnsAsync(response);

        // Act
        var result = await _sut.SearchCatalogAsync("test");

        // Assert
        result.ShouldBe("There was an error while searching the catalog. Please try again later.");
    }

    [Test]
    public async Task GivenApiReturnsEmptyList_WhenSearchCatalog_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        var pagedResult = new PagedResult<Book>([], 1, 10, 0);
        var response = new ApiResponse<PagedResult<Book>>(
            new(HttpStatusCode.OK),
            pagedResult,
            new()
        );

        _catalogApi.Setup(x => x.ListBooksAsync(It.IsAny<string?>())).ReturnsAsync(response);

        // Act
        var result = await _sut.SearchCatalogAsync("nonexistent");

        // Assert
        result.ShouldBe(
            "We couldn't find any books matching your description. Please try again with a different description."
        );
    }

    [Test]
    public async Task GivenApiReturnsBooks_WhenSearchCatalog_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var books = new List<Book>
        {
            new(
                Guid.CreateVersion7(),
                "Clean Code",
                "A book about writing clean code",
                "https://example.com/image.jpg",
                29.99m,
                null,
                new(Guid.CreateVersion7(), "Programming"),
                new(Guid.CreateVersion7(), "Prentice Hall"),
                [new(Guid.CreateVersion7(), "Robert C. Martin")],
                4.5,
                120
            ),
        };

        var pagedResult = new PagedResult<Book>(books, 1, 10, 1);
        var response = new ApiResponse<PagedResult<Book>>(
            new(HttpStatusCode.OK),
            pagedResult,
            new()
        );

        _catalogApi.Setup(x => x.ListBooksAsync(It.IsAny<string?>())).ReturnsAsync(response);

        // Act
        var result = await _sut.SearchCatalogAsync("clean code");

        // Assert
        result.ShouldContain("Clean Code");
        result.ShouldContain("Robert C. Martin");
        result.ShouldContain("29.99");
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenGetBook_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var response = new ApiResponse<Book>(new(HttpStatusCode.InternalServerError), null, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldBe("There was an error retrieving the book. Please try again later.");
    }

    [Test]
    public async Task GivenApiReturnsNull_WhenGetBook_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var response = new ApiResponse<Book>(new(HttpStatusCode.OK), null, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldBe($"No book found with ID {id}.");
    }

    [Test]
    public async Task GivenApiReturnsBook_WhenGetBook_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var book = new Book(
            id,
            "Clean Code",
            "A book about writing clean code",
            "https://example.com/image.jpg",
            29.99m,
            null,
            new(Guid.CreateVersion7(), "Programming"),
            new(Guid.CreateVersion7(), "Prentice Hall"),
            [new(Guid.CreateVersion7(), "Robert C. Martin")],
            4.5,
            120
        );

        var response = new ApiResponse<Book>(new(HttpStatusCode.OK), book, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldContain("Clean Code");
        result.ShouldContain("29.99");
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenListCategories_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var response = new ApiResponse<List<Category>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldBe("There was an error retrieving categories. Please try again later.");
    }

    [Test]
    public async Task GivenApiReturnsEmptyList_WhenListCategories_ThenShouldReturnNoCategories()
    {
        // Arrange
        var response = new ApiResponse<List<Category>>(new(HttpStatusCode.OK), [], new());

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldBe("No categories are currently available.");
    }

    [Test]
    public async Task GivenApiReturnsNull_WhenListCategories_ThenShouldReturnNoCategories()
    {
        // Arrange
        var response = new ApiResponse<List<Category>>(new(HttpStatusCode.OK), null, new());

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldBe("No categories are currently available.");
    }

    [Test]
    public async Task GivenApiReturnsCategories_WhenListCategories_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var categories = new List<Category>
        {
            new(Guid.CreateVersion7(), "Programming"),
            new(Guid.CreateVersion7(), "Fiction"),
        };

        var response = new ApiResponse<List<Category>>(new(HttpStatusCode.OK), categories, new());

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListCategoriesAsync();

        // Assert
        result.ShouldContain("Programming");
        result.ShouldContain("Fiction");
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenListAuthors_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var response = new ApiResponse<List<Author>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldBe("There was an error retrieving authors. Please try again later.");
    }

    [Test]
    public async Task GivenApiReturnsEmptyList_WhenListAuthors_ThenShouldReturnNoAuthors()
    {
        // Arrange
        var response = new ApiResponse<List<Author>>(new(HttpStatusCode.OK), [], new());

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldBe("No authors are currently available.");
    }

    [Test]
    public async Task GivenApiReturnsNull_WhenListAuthors_ThenShouldReturnNoAuthors()
    {
        // Arrange
        var response = new ApiResponse<List<Author>>(new(HttpStatusCode.OK), null, new());

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldBe("No authors are currently available.");
    }

    [Test]
    public async Task GivenApiReturnsAuthors_WhenListAuthors_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var authors = new List<Author>
        {
            new(Guid.CreateVersion7(), "Robert C. Martin"),
            new(Guid.CreateVersion7(), "Martin Fowler"),
        };

        var response = new ApiResponse<List<Author>>(new(HttpStatusCode.OK), authors, new());

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.ListAuthorsAsync();

        // Assert
        result.ShouldContain("Robert C. Martin");
        result.ShouldContain("Martin Fowler");
    }
}
