using System.Net;
using BookWorm.McpTools.Models;
using BookWorm.McpTools.Resources;
using BookWorm.McpTools.Services;
using ModelContextProtocol;
using Refit;

namespace BookWorm.McpTools.UnitTests.Resources;

public sealed class CatalogResourceProviderTests
{
    private readonly Mock<ICatalogApi> _catalogApi = new();
    private readonly CatalogResourceProvider _sut;

    public CatalogResourceProviderTests()
    {
        _sut = new(_catalogApi.Object);
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenGetCategories_ThenShouldReturnEmptyArray()
    {
        // Arrange
        var response = new ApiResponse<List<Category>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        result.ShouldBe("[]");
    }

    [Test]
    public async Task GivenApiReturnsEmptyList_WhenGetCategories_ThenShouldReturnEmptyArray()
    {
        // Arrange
        var response = new ApiResponse<List<Category>>(new(HttpStatusCode.OK), [], new());

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        result.ShouldBe("[]");
    }

    [Test]
    public async Task GivenApiReturnsCategories_WhenGetCategories_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var categories = new List<Category>
        {
            new(Guid.CreateVersion7(), "Science Fiction"),
            new(Guid.CreateVersion7(), "Fantasy"),
        };

        var response = new ApiResponse<List<Category>>(new(HttpStatusCode.OK), categories, new());

        _catalogApi.Setup(x => x.ListCategoriesAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("Science Fiction");
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenGetAuthors_ThenShouldReturnEmptyArray()
    {
        // Arrange
        var response = new ApiResponse<List<Author>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.GetAuthorsAsync();

        // Assert
        result.ShouldBe("[]");
    }

    [Test]
    public async Task GivenApiReturnsAuthors_WhenGetAuthors_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var authors = new List<Author>
        {
            new(Guid.CreateVersion7(), "Frank Herbert"),
            new(Guid.CreateVersion7(), "Isaac Asimov"),
        };

        var response = new ApiResponse<List<Author>>(new(HttpStatusCode.OK), authors, new());

        _catalogApi.Setup(x => x.ListAuthorsAsync()).ReturnsAsync(response);

        // Act
        var result = await _sut.GetAuthorsAsync();

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("Frank Herbert");
    }

    [Test]
    public async Task GivenApiReturnsBook_WhenGetBook_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var book = new Book(
            id,
            "Dune",
            "Epic sci-fi",
            null,
            29.99m,
            null,
            new(Guid.CreateVersion7(), "Sci-Fi"),
            new(Guid.CreateVersion7(), "Publisher A"),
            [new(Guid.CreateVersion7(), "Frank Herbert")],
            4.8,
            120
        );

        var response = new ApiResponse<Book>(new(HttpStatusCode.OK), book, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookAsync(id);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("Dune");
        result.ShouldContain("Frank Herbert");
    }

    [Test]
    public async Task GivenApiReturnsNotFound_WhenGetBook_ThenShouldThrowMcpException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var response = new ApiResponse<Book>(new(HttpStatusCode.NotFound), null, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act & Assert
        await Should.ThrowAsync<McpException>(() => _sut.GetBookAsync(id));
    }

    [Test]
    public async Task GivenApiReturnsNullContent_WhenGetBook_ThenShouldThrowMcpException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var response = new ApiResponse<Book>(new(HttpStatusCode.OK), null, new());

        _catalogApi.Setup(x => x.GetBookAsync(id)).ReturnsAsync(response);

        // Act & Assert
        await Should.ThrowAsync<McpException>(() => _sut.GetBookAsync(id));
    }
}
