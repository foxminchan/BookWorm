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
}
