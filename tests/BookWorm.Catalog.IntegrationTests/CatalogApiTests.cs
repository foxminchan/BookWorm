using System.Net;
using System.Net.Http.Json;
using Asp.Versioning.Http;
using BookWorm.Catalog.Features.Categories.Create;

namespace BookWorm.Catalog.IntegrationTests;

public sealed class CatalogApiTests : IClassFixture<CatalogApiFixture>
{
    private readonly HttpClient _httpClient;

    public CatalogApiTests(CatalogApiFixture fixture)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new(1.0));
        _httpClient = fixture.CreateDefaultClient(handler);
    }

    [Fact]
    public async Task CreateCategory()
    {
        // Arrange
        var category = new CreateCategoryRequest("Test Category");

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/categories", category);
        var statusCode = response.StatusCode;

        // Assert
        statusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetCategories()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/categories");
        var statusCode = response.StatusCode;

        // Assert
        statusCode.Should().Be(HttpStatusCode.OK);
    }
}
