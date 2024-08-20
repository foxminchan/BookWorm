using System.Net;
using System.Net.Http.Json;
using Asp.Versioning;

namespace BookWorm.Basket.IntegrationTests;

public sealed class BasketApiTests : IClassFixture<BasketApiFixture>
{
    private static readonly ApiVersion _apiVersion = new(1, 0);
    private readonly string _apiPrefix = $"/api/v{_apiVersion.MajorVersion}";
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public BasketApiTests(BasketApiFixture fixture)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), _apiVersion);
        _httpClient = fixture.CreateDefaultClient(handler);
    }

    [Fact]
    public async Task ReduceItemQuantity()
    {
        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/{_apiPrefix}/baskets/{Guid.NewGuid()}/reduce-item-quantity",
            new { },
            _jsonSerializerOptions);
        var statusCode = response.StatusCode;

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItem()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            $"/{_apiPrefix}/baskets/{Guid.NewGuid()}/items",
            CancellationToken.None);
        var statusCode = response.StatusCode;

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBasket()
    {
        // Act
        var response = await _httpClient.GetAsync(
            $"/{_apiPrefix}/baskets",
            CancellationToken.None);
        var statusCode = response.StatusCode;

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
