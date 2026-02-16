using System.Net;
using System.Net.Mime;
using BookWorm.Catalog.IntegrationTests.Fixtures;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Features.Books;

[ClassDataSource<CatalogAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class ListBooksTests(CatalogAppFixture fixture)
{
    [Test]
    [DisplayName("GET /api/v1/books should return success status code")]
    public async Task GivenCatalogService_WhenListingBooks_ThenReturnsSuccess()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/books");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
    }

    [Test]
    [DisplayName("GET /api/v1/books with pagination should return success")]
    public async Task GivenPaginationParams_WhenListingBooks_ThenReturnsSuccess()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/books?pageIndex=0&pageSize=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
    }

    [Test]
    [DisplayName("GET /api/v1/books with price range filter should return success")]
    public async Task GivenPriceFilter_WhenListingBooks_ThenReturnsSuccess()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/books?minPrice=0&maxPrice=100");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
    }
}
