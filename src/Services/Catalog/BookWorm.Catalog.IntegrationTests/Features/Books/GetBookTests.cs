using System.Net;
using System.Net.Mime;
using BookWorm.Catalog.IntegrationTests.Fixtures;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Features.Books;

[ClassDataSource<CatalogAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class GetBookTests(CatalogAppFixture fixture)
{
    [Test]
    [DisplayName("GET /api/v1/books/{id} with non-existent ID should return NotFound")]
    public async Task GivenNonExistentBookId_WhenGettingBook_ThenReturnsNotFound()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/v1/books/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(
            MediaTypeNames.Application.ProblemJson
        );
    }

    [Test]
    [DisplayName(
        "GET /api/v1/books/{id} with invalid GUID format should return BadRequest or NotFound"
    )]
    public async Task GivenInvalidIdFormat_WhenGettingBook_ThenReturnsClientError()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/books/not-a-guid");

        // Assert — route constraint {id:guid} should reject the invalid format
        ((int)response.StatusCode).ShouldBeInRange(400, 499);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(
            MediaTypeNames.Application.ProblemJson
        );
    }
}
