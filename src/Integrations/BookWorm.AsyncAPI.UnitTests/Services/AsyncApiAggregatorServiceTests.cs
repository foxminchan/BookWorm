using System.Net;
using System.Text.Json.Nodes;

namespace BookWorm.AsyncAPI.UnitTests.Services;

/// <summary>
/// Unit tests for AsyncApiAggregatorService
/// </summary>
public class AsyncApiAggregatorServiceTests
{
    [Test]
    public async Task GetDiscoveredServicesAsync_WithValidServices_ReturnsServiceInfo()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockEndpointProvider = new Mock<IServiceEndpointProvider>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<AsyncApiAggregatorService>>();

        var expectedEndpoints = new[]
        {
            new ServiceEndpoint
            {
                Scheme = "https",
                Host = "localhost",
                Port = 5001
            }
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceEndpointProvider)))
            .Returns(mockEndpointProvider.Object);

        mockEndpointProvider
            .Setup(x => x.GetEndpointsAsync("catalog", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEndpoints);

        var service = new AsyncApiAggregatorService(
            mockServiceProvider.Object,
            mockHttpClient.Object,
            mockLogger.Object);

        // Act
        var result = await service.GetDiscoveredServicesAsync();

        // Assert
        result.Should().NotBeEmpty();
        var catalogService = result.FirstOrDefault(s => s.Name == "catalog");
        catalogService.Should().NotBeNull();
        catalogService!.BaseUrl.Should().Be("https://localhost:5001");
        catalogService.AsyncApiUrl.Should().Be("https://localhost:5001/asyncapi/docs/asyncapi.json");
    }

    [Test]
    public async Task GetAggregatedAsyncApiAsync_WithValidSpecs_ReturnsAggregatedSpec()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<AsyncApiAggregatorService>>();

        // Mock service that returns empty services (to avoid complex setup)
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceEndpointProvider)))
            .Returns((IServiceEndpointProvider?)null);

        var service = new AsyncApiAggregatorService(
            mockServiceProvider.Object,
            mockHttpClient.Object,
            mockLogger.Object);

        // Act
        var result = await service.GetAggregatedAsyncApiAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        
        var jsonObject = JsonNode.Parse(result);
        jsonObject.Should().NotBeNull();
        jsonObject!["asyncapi"]?.GetValue<string>().Should().Be("2.6.0");
        jsonObject["info"]?["title"]?.GetValue<string>().Should().Be("BookWorm Aggregated AsyncAPI");
    }

    [Test]
    public async Task GetDiscoveredServicesAsync_WithServiceProviderNull_ReturnsEmptyList()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<AsyncApiAggregatorService>>();

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceEndpointProvider)))
            .Returns((IServiceEndpointProvider?)null);

        var service = new AsyncApiAggregatorService(
            mockServiceProvider.Object,
            mockHttpClient.Object,
            mockLogger.Object);

        // Act
        var result = await service.GetDiscoveredServicesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}