using System.Net;
using System.Net.Mime;
using BookWorm.Catalog.IntegrationTests.Fixtures;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Features.Publishers;

[ClassDataSource<CatalogAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class ListPublishersTests(CatalogAppFixture fixture)
{
    [Test]
    [DisplayName("GET /api/v1/publishers should return success status code")]
    public async Task GivenCatalogService_WhenListingPublishers_ThenReturnsSuccess()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/publishers");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
    }
}
